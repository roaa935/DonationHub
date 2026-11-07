
using DonationHub.Application.Authentication;
using DonationHub.Application.Options;
using DonationHub.Application.Persistence;
using DonationHub.Domain.Entities;
using DonationHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
namespace DonationHub.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IGenericRepository<ApplicationUser> _users;
    private readonly IGenericRepository<RefreshToken> _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly JwtOptions _options;

    public AuthService(
        UserManager<ApplicationIdentityUser> userManager,
        IJwtTokenService jwt,
        IRefreshTokenService refresh,
        IOptions<JwtOptions> options,
        IGenericRepository<ApplicationUser> users,
        IGenericRepository<RefreshToken> refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwt = jwt;
        _refresh = refresh;
        _options = options.Value;
        _users = users;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim();

        var existing = await _userManager.FindByEmailAsync(email);

        if (existing != null)
            throw new AuthException("Auth.EmailAlreadyExists", 409);

        var language = string.IsNullOrWhiteSpace(request.PreferredLanguage)
            ? "en"
            : request.PreferredLanguage.Trim().ToLowerInvariant();

        if (language != "en" && language != "ar")
            throw new AuthException("Auth.InvalidLanguage", 400);

        var now = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        var identityUser = new ApplicationIdentityUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PreferredLanguage = language,
            IsActive = true,
            EmailConfirmed = true
        };

        AuthResponse? response = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var result = await _userManager.CreateAsync(
                identityUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Where(x => x.Code.StartsWith("Password"))
                    .Select(x => x.Description);

                throw new AuthException(
                    string.Join("; ", errors), 400);
            }

            var domainUser = ApplicationUser.Create(
                userId,
                email,
                email,
                request.FullName.Trim(),
                language,
                now);

            await _users.AddAsync(domainUser);

            var roleResult = await _userManager.AddToRoleAsync(
                identityUser, "Donor");

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(x => x.Description)));

            response = await CreateAuthResponseAsync(
                identityUser, domainUser);

            
            await _unitOfWork.CompleteAsync();
        });

        return response!;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim();

        var identityUser = await _userManager.FindByEmailAsync(email);

        if (identityUser == null ||
            !await _userManager.CheckPasswordAsync(
                identityUser, request.Password))
        {
            throw new AuthException("Auth.InvalidCredentials", 401);
        }

        if (!identityUser.IsActive)
            throw new AuthException("Auth.AccountDeactivated", 403);

        var domainUser = await _users.GetByIdAsync(identityUser.Id);

        if (domainUser == null)
            throw new InvalidOperationException(
                "User domain record was not found.");

        var response = await CreateAuthResponseAsync(
            identityUser, domainUser);

        await _unitOfWork.CompleteAsync();

        return response;
    }

    public async Task<UserProfileResponse> GetMeAsync(Guid userId)
    {
        var identityUser = await _userManager.FindByIdAsync(
            userId.ToString());

        if (identityUser == null)
            throw new AuthException("Auth.InvalidCredentials", 401);

        var roles = await _userManager.GetRolesAsync(identityUser);

        return new UserProfileResponse(
            identityUser.Id,
            identityUser.Email!,
            identityUser.FullName,
            identityUser.PreferredLanguage,
            roles.ToArray());
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationIdentityUser identityUser,
        ApplicationUser domainUser)
    {
        var roles = await _userManager.GetRolesAsync(identityUser);

        var accessToken = _jwt.GenerateAccessToken(
            domainUser, roles.ToArray());

        var (plainToken, hash) = _refresh.Generate();

        var now = DateTime.UtcNow;
        var refreshExpiry = now.AddDays(
            _options.RefreshTokenExpiryDays);

        var refreshEntity = RefreshToken.Create(
            domainUser.Id, hash, now, refreshExpiry);

        await _refreshTokens.AddAsync(refreshEntity);

        return new AuthResponse(
            accessToken,
            plainToken,
            now.AddMinutes(_options.ExpiryMinutes),
            refreshExpiry);
    }


    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new AuthException("Auth.RefreshTokenInvalid", 401);

        var hash = _refresh.Hash(request.RefreshToken);

        var oldToken = await _refreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == hash);

        var now = DateTime.UtcNow;

        if (oldToken == null || !oldToken.IsActive(now))
            throw new AuthException("Auth.RefreshTokenInvalid", 401);

        var domainUser = await _users.GetByIdAsync(oldToken.UserId);

        if (domainUser == null)
            throw new AuthException("Auth.RefreshTokenInvalid", 401);

        var identityUser = await _userManager.FindByIdAsync(
            oldToken.UserId.ToString());

        if (identityUser == null)
            throw new AuthException("Auth.RefreshTokenInvalid", 401);

        if (!identityUser.IsActive || !domainUser.IsActive)
            throw new AuthException("Auth.AccountDeactivated", 403);

        AuthResponse? response = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Generate the replacement token.
            var (plainToken, newHash) = _refresh.Generate();

            var newExpiry = now.AddDays(_options.RefreshTokenExpiryDays);

            var newToken = RefreshToken.Create(
                domainUser.Id,
                newHash,
                now,
                newExpiry);

            // Revoke the old token and link it to its replacement.
            oldToken.Revoke(now, newToken.Id);

            await _refreshTokens.AddAsync(newToken);

            var roles = await _userManager.GetRolesAsync(identityUser);

            var accessToken = _jwt.GenerateAccessToken(
                domainUser,
                roles.ToArray());

            response = new AuthResponse(
                accessToken,
                plainToken,
                now.AddMinutes(_options.ExpiryMinutes),
                newExpiry);
        });

        return response!;
    }

    public async Task LogoutAsync(Guid userId, LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new AuthException("Auth.RefreshTokenInvalid", 401);

        var hash = _refresh.Hash(request.RefreshToken);

        var token = await _refreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == hash);

        var now = DateTime.UtcNow;

        // A user must not be able to revoke another user's token.
        if (token == null ||
            token.UserId != userId ||
            !token.IsActive(now))
        {
            throw new AuthException("Auth.RefreshTokenInvalid", 401);
        }

        token.Revoke(now);

        await _unitOfWork.CompleteAsync();
    }

    public async Task<StaffCreationResponse> CreateStaffAsync(
        CreateStaffRequest request)
    {
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new AuthException("Auth.InvalidFullName", 400);

        var role = request.Role.Trim();

        if (role != Roles.Admin && role != Roles.CampaignManager)
            throw new AuthException("Auth.InvalidStaffRole", 400);

        var existing = await _userManager.FindByEmailAsync(email);

        if (existing != null)
            throw new AuthException("Auth.EmailAlreadyExists", 409);

        var temporaryPassword = GenerateTemporaryPassword();

        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var identityUser = new ApplicationIdentityUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PreferredLanguage = "en",
            IsActive = true,
            EmailConfirmed = true
        };

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var createResult = await _userManager.CreateAsync(
                identityUser,
                temporaryPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to create staff user: {errors}");
            }

            var domainUser = ApplicationUser.Create(
                userId,
                email,
                email,
                request.FullName.Trim(),
                "en",
                now);

            await _users.AddAsync(domainUser);

            var roleResult = await _userManager.AddToRoleAsync(
                identityUser,
                role);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"Failed to assign staff role: {errors}");
            }

            await _unitOfWork.CompleteAsync();
        });

        return new StaffCreationResponse(
            userId,
            email,
            identityUser.FullName,
            role,
            temporaryPassword);
    }

    private static string GenerateTemporaryPassword()
    {
        // Cryptographically secure random password.
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%";

        Func<int, int> random = RandomNumberGenerator.GetInt32;

        var chars = new List<char>
    {
        upper[random(upper.Length)],
        lower[random(lower.Length)],
        digits[random(digits.Length)],
        special[random(special.Length)]
    };

        const string all = upper + lower + digits + special;

        while (chars.Count < 16)
            chars.Add(all[random(all.Length)]);

        // Securely shuffle the characters.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = random(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

}
