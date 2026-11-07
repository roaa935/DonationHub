
namespace DonationHub.Application.Authentication;

public interface IRefreshTokenService
{
    (string Token, string Hash) Generate();

    string Hash(string token);
}
