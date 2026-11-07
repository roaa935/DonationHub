using DonationHub.Application.Authentication;
public record RegisterRequest(
    string Email,
    string FullName,
    string Password,
    string PreferredLanguage = "en");

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record LogoutRequest(
    string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string PreferredLanguage,
    IReadOnlyCollection<string> Roles);
