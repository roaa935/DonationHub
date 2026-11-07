using DonationHub.Domain.Entities;

namespace DonationHub.Application.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles);
}