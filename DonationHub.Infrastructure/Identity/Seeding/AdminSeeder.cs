
using DonationHub.Application.Options;
using DonationHub.Application.Persistence;
using DonationHub.Domain.Entities;
using DonationHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DonationHub.Infrastructure.Seeding;

public class AdminSeeder
{
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IGenericRepository<ApplicationUser> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SeedAdminOptions _options;

    public AdminSeeder(
        UserManager<ApplicationIdentityUser> userManager,
        IGenericRepository<ApplicationUser> users,
        IUnitOfWork unitOfWork,
        IOptions<SeedAdminOptions> adminOptions)
    {
        _userManager = userManager;
        _users = users;
        _unitOfWork = unitOfWork;
        _options = adminOptions.Value;
    }

    public async Task SeedAsync()
    {
        var email = _options.Email.Trim();

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Find the Identity user by normalized email.
            var identityUser = await _userManager.FindByEmailAsync(email);

            // Create the Identity user if it doesn't exist.
            if (identityUser == null)
            {
                identityUser = new ApplicationIdentityUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    FullName = _options.FullName.Trim(),
                    PreferredLanguage = "en",
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(
                    identityUser,
                    _options.Password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        createResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create initial Admin: {errors}");
                }
            }

            // Ensure the matching Domain user exists.
            var domainUser = await _users.GetByIdAsync(identityUser.Id);

            if (domainUser == null)
            {
                domainUser = ApplicationUser.Create(
                    identityUser.Id,
                    identityUser.Email!,
                    identityUser.UserName!,
                    identityUser.FullName,
                    identityUser.PreferredLanguage,
                    DateTime.UtcNow);

                await _users.AddAsync(domainUser);
            }

            // Ensure the user has the Admin role.
            if (!await _userManager.IsInRoleAsync(
                    identityUser, Roles.Admin))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    identityUser, Roles.Admin);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Admin role: {errors}");
                }
            }
        });
    }
}
