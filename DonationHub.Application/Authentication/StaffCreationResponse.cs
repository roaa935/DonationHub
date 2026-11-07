namespace DonationHub.Application.Authentication;

public record StaffCreationResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string TemporaryPassword);
