using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using DonationHub.Domain.ValueObjects;

namespace DonationHub.Domain.Entities;

public class Campaign : Entity , ISoftDeletable
{
    public string TitleAr { get; private set; } = null!;
    public string TitleEn { get; private set; } = null!;
    public string DescriptionAr { get; private set; } = null!;
    public string DescriptionEn { get; private set; } = null!;

    public Money GoalAmount { get; private set; }
    public Money RaisedAmount { get; private set; }

    public DateTime EndDate { get; private set; }

    public Guid CategoryId { get; private set; }

    public string? BannerFileName { get; private set; }

    public CampaignStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    //navigation properties
    public Category Category { get; private set; } = null!;
    public ApplicationUser CreatedByUser { get; private set; } = null!;
    public ICollection<Donation> Donations { get; private set; } = new List<Donation>();

    // EF Core concurrency token
    public byte[] RowVersion { get; private set; } = null!;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get ; set; }

    private Campaign() { }

    private Campaign( string titleAr, string titleEn,string descriptionAr, string descriptionEn, Money goalAmount, DateTime endDate,  Guid categoryId, string? bannerFileName, Guid createdByUserId,  DateTime nowUtc)
        : base(Guid.NewGuid(), nowUtc)
    {
        TitleAr = titleAr;
        TitleEn = titleEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        GoalAmount = goalAmount;
        RaisedAmount = Money.Create(0, goalAmount.Currency);
        EndDate = endDate;
        CategoryId = categoryId;
        BannerFileName = bannerFileName;
        CreatedByUserId = createdByUserId;
        Status = CampaignStatus.Active;
        IsDeleted = false;
    }

    public static Campaign Create(
        string titleAr,
        string titleEn,
        string descriptionAr,
        string descriptionEn,
        Money goalAmount,
        DateTime endDate,
        Guid categoryId,
        string? bannerFileName,
        Guid createdByUserId,
        DateTime nowUtc)
    {
        ValidateText(titleAr, nameof(titleAr));
        ValidateText(titleEn, nameof(titleEn));
        ValidateText(descriptionAr, nameof(descriptionAr));
        ValidateText(descriptionEn, nameof(descriptionEn));

        if (goalAmount.Amount <= 0)
            throw new ArgumentException("Goal amount must be positive.");

        if (endDate <= nowUtc)
            throw new ArgumentException("End date must be in the future.");

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Invalid category ID.");

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Invalid creator ID.");

        return new Campaign(
            titleAr, titleEn,
            descriptionAr, descriptionEn,
            goalAmount, endDate,
            categoryId, bannerFileName,
            createdByUserId, nowUtc);
    }

    public void UpdateGoalAmount(Money newGoalAmount, DateTime nowUtc)
    {
        EnsureNotFinal();

        if (newGoalAmount.Amount <= 0)
            throw new ArgumentException("Goal amount must be positive.");

        if (newGoalAmount.Amount < RaisedAmount.Amount)
            throw new InvalidOperationException(
                "Goal cannot be lower than the raised amount.");

        GoalAmount = newGoalAmount;
        UpdateTimestamp(nowUtc);


        if (RaisedAmount.Amount >= GoalAmount.Amount)
            MarkCompleted(nowUtc);
    }

    public void ChangeEndDate(DateTime newEndDate, DateTime nowUtc)
    {
        EnsureNotFinal();

        if (newEndDate <= nowUtc)
            throw new ArgumentException("End date must be in the future.");

        EndDate = newEndDate;
        UpdateTimestamp(nowUtc);
    }

    public void UpdateDetails(
        string titleAr,
        string titleEn,
        string descriptionAr,
        string descriptionEn,
        DateTime nowUtc)
    {
        EnsureNotFinal();

        ValidateText(titleAr, nameof(titleAr));
        ValidateText(titleEn, nameof(titleEn));
        ValidateText(descriptionAr, nameof(descriptionAr));
        ValidateText(descriptionEn, nameof(descriptionEn));

        TitleAr = titleAr;
        TitleEn = titleEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        UpdateTimestamp(nowUtc);

    }

    public void EnsureCanAcceptDonation(DateTime nowUtc)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Campaign is deleted.");

        if (Status != CampaignStatus.Active)
            throw new InvalidOperationException(
                "Campaign is not active.");

        if (EndDate <= nowUtc)
            throw new InvalidOperationException(
                "Campaign has expired.");
    }

    public void RecordSuccessfulDonation(Money amount,DateTime nowUtc)
    {
        EnsureCanAcceptDonation(nowUtc);

        if (amount.Amount <= 0)
            throw new ArgumentException("Donation must be positive.");

        RaisedAmount = RaisedAmount.Add(amount);
        UpdateTimestamp(nowUtc);

        if (RaisedAmount.Amount >= GoalAmount.Amount)
            MarkCompleted(nowUtc);
    }

    public void MarkCompleted(DateTime nowUtc)
    {
        EnsureNotFinal();

        Status = CampaignStatus.Completed;
        UpdateTimestamp(nowUtc);
    }

    public void MarkExpired(DateTime nowUtc)
    {
        EnsureNotFinal();

        if (EndDate > nowUtc)
            throw new InvalidOperationException(
                "Campaign has not reached its end date.");

        Status = CampaignStatus.Expired;
        UpdateTimestamp(nowUtc);
    }

    public void SoftDelete(bool hasDonations, DateTime nowUtc)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Campaign is already deleted.");

        if (hasDonations)
            throw new InvalidOperationException(
                "Campaign with donations cannot be deleted.");

        IsDeleted = true;
        DeletedAt = nowUtc;
        UpdateTimestamp(nowUtc);
    }

    private void EnsureNotFinal()
    {
        if (Status != CampaignStatus.Active)
            throw new InvalidOperationException(
                "Final campaign status cannot be changed.");
    }

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                $"{parameterName} is required.", parameterName);
    }
    public void Restor()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Campaign is not deleted.");

        IsDeleted = true;
        DeletedAt = default;
    }
}