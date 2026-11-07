using Microsoft.AspNetCore.Identity;

namespace DonationHub.Infrastructure.Identity;

public class ApplicationIdentityUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;

    public string PreferredLanguage { get; set; } = "en";

    public bool IsActive { get; set; } = true;


}