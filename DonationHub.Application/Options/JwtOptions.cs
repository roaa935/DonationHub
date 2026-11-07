
using System.ComponentModel.DataAnnotations;

namespace DonationHub.Application.Options;

public class JwtOptions
{
    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; }

    [Range(1, 365)]
    public int RefreshTokenExpiryDays { get; set; }
}
