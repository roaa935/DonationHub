namespace DonationHub.Application.Authentication;

public record CreateStaffRequest(
    string Email,
    string FullName,
    string Role
    );
