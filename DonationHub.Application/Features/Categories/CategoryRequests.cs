
using System.ComponentModel.DataAnnotations;

namespace DonationHub.Application.Features.Categories;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = null!;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
}

public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameAr { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NameEn { get; set; } = null!;

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
}
