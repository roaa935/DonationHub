
using System.ComponentModel.DataAnnotations;

namespace DonationHub.Application.Options;

public class SeedAdminOptions
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}
