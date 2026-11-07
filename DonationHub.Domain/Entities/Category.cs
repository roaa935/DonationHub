
using DonationHub.Domain.Common;

namespace DonationHub.Domain.Entities;

public class Category : Entity
{
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;

    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }

    public ICollection<Campaign> Campaigns { get; private set; }
        = new List<Campaign>();

    private Category()
    {
    }

    private Category(
        string nameAr,
        string nameEn,
        string? descriptionAr,
        string? descriptionEn,
        DateTime nowUtc) : base(Guid.NewGuid(), nowUtc)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
    }

    public static Category Create(
        string nameAr,
        string nameEn,
        string? descriptionAr,
        string? descriptionEn,
        DateTime nowUtc)
    {
        ValidateName(nameAr, nameof(nameAr));
        ValidateName(nameEn, nameof(nameEn));

        return new Category(
            nameAr.Trim(),
            nameEn.Trim(),
            NormalizeDescription(descriptionAr),
            NormalizeDescription(descriptionEn),
            nowUtc);
    }

    public void Update(
        string nameAr,
        string nameEn,
        string? descriptionAr,
        string? descriptionEn,
        DateTime nowUtc)
    {
        ValidateName(nameAr, nameof(nameAr));
        ValidateName(nameEn, nameof(nameEn));

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        DescriptionAr = NormalizeDescription(descriptionAr);
        DescriptionEn = NormalizeDescription(descriptionEn);

        UpdateTimestamp(nowUtc);
    }

    private static void ValidateName(string name, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                $"{propertyName} is required.", propertyName);

        if (name.Trim().Length > 100)
            throw new ArgumentException(
                $"{propertyName} cannot exceed 100 characters.", propertyName);
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }
}
