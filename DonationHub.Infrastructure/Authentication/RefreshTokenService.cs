
using System.Security.Cryptography;
using System.Text;
using DonationHub.Application.Authentication;

namespace DonationHub.Infrastructure.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    public (string Token, string Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);

        return (token, Hash(token));
    }

    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
