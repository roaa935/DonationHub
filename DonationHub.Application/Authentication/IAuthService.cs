
namespace DonationHub.Application.Authentication;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<UserProfileResponse> GetMeAsync(Guid userId);

    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);

    Task LogoutAsync(Guid userId, LogoutRequest request);
    Task<StaffCreationResponse> CreateStaffAsync(CreateStaffRequest request);
}
