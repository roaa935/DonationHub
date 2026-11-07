using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using DonationHub.Domain.ValueObjects;

namespace DonationHub.Domain.Entities;

public class Donation : Entity
{
    public Guid CampaignId { get; private set; }
    public Guid DonorUserId { get; private set; }

    public Money Amount { get; private set; }

    public DonationStatus Status { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public string? FailureReason { get; private set; }
    public string? ReceiptNumber { get; private set; }
    public string? ReceiptFileName { get; private set; }
    //navigation properties
    public Campaign Campaign { get; private set; } = null!;
    public ApplicationUser DonorUser { get; private set; } = null!;
    private Donation() { }

    private Donation( Guid campaignId, Guid donorUserId, Money amount, DateTime nowUtc): base(Guid.NewGuid(), nowUtc)
    {
        CampaignId = campaignId;
        DonorUserId = donorUserId;
        Amount = amount;
        Status = DonationStatus.Pending;
    }

    public static Donation Create(
        Guid campaignId,
        Guid donorUserId,
        Money amount,
        decimal minAmount,
        decimal maxAmount,
        DateTime nowUtc)
    {
        if (campaignId == Guid.Empty)
            throw new ArgumentException("Invalid campaign ID.");

        if (donorUserId == Guid.Empty)
            throw new ArgumentException("Invalid donor ID.");

        if (minAmount <= 0 || maxAmount < minAmount)
            throw new ArgumentException("Invalid donation limits.");

        if (amount.Amount < minAmount || amount.Amount > maxAmount)
            throw new ArgumentException( $"Donation must be between {minAmount} and {maxAmount}.");

        return new Donation(campaignId, donorUserId, amount, nowUtc);
    }

    public void MarkSucceeded(
        DateTime nowUtc,
        string? receiptNumber = null,
        string? receiptFileName = null)
    {
        EnsurePending();

        Status = DonationStatus.Succeeded;
        CompletedAt = nowUtc;
        ReceiptNumber = receiptNumber;
        ReceiptFileName = receiptFileName;
        UpdateTimestamp(nowUtc);
    }

    public void MarkFailed(string failureReason, DateTime nowUtc)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required.");

        Status = DonationStatus.Failed;
        CompletedAt = nowUtc;
        FailureReason = failureReason;
        UpdateTimestamp(nowUtc);
    }

    private void EnsurePending()
    {
        if (Status != DonationStatus.Pending)
            throw new InvalidOperationException(
                "A final donation status cannot be changed.");
        
    }
}