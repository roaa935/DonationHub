
namespace DonationHub.Application.Authentication;

public class AuthException(string code, int statusCode)
    : Exception(code)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
