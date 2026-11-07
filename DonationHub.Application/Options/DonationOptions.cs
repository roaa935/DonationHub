
using System.ComponentModel.DataAnnotations;

namespace DonationHub.Application.Options;

public class DonationOptions
{
    [Range(typeof(decimal), "0.01", "10000.00")]
    public decimal MinAmount { get; set; } = 1.00m;

    [Range(typeof(decimal), "0.01", "10000.00")]
    public decimal MaxAmount { get; set; } = 10000.00m;

    [Range(1, 1440)]
    public int PendingTimeoutMinutes { get; set; } = 30;

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string DefaultCurrency { get; set; } = "USD";
}
