namespace DonationHub.Application.Features.Categories;

public record CategoryResponse(
    Guid Id,
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn
);
