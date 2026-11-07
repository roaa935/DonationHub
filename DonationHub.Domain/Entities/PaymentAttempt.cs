using DonationHub.Domain.Common;
using DonationHub.Domain.Enums;
using DonationHub.Domain.ValueObjects;

namespace DonationHub.Domain.Entities;

public class PaymentAttempt : Entity
{
    public Guid DonationId { get; private set; }

    public string StripeCheckoutSessionId { get; private set; } = null!;

    public string? StripePaymentIntentId { get; private set; }

    public Money Amount { get; private set; }

    public PaymentAttemptStatus Status { get; private set; }

    

    public string? FailureCode { get; private set; }

    private PaymentAttempt() { }

    private PaymentAttempt(Guid donationId,string stripeCheckoutSessionId,Money amount,DateTime nowUtc): base(Guid.NewGuid(), nowUtc)
    {
        DonationId = donationId;
        StripeCheckoutSessionId = stripeCheckoutSessionId;
        Amount = amount;
        Status = PaymentAttemptStatus.Created;
        
    }

    public static PaymentAttempt Create(Guid donationId,string stripeCheckoutSessionId,Money amount,DateTime nowUtc)
    {
        if (donationId == Guid.Empty)
            throw new ArgumentException("Invalid donation ID.");

        if (string.IsNullOrWhiteSpace(stripeCheckoutSessionId))
            throw new ArgumentException("Stripe session ID is required.");

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        return new PaymentAttempt(donationId,stripeCheckoutSessionId,amount,nowUtc);
    }

    public void MarkSucceeded(string? paymentIntentId,DateTime nowUtc)
    {
        EnsureCreated();

        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new ArgumentException("Payment intent ID is required.");

        Status = PaymentAttemptStatus.Succeeded;
        StripePaymentIntentId = paymentIntentId;
        UpdateTimestamp(nowUtc);
    }

    public void MarkFailed(string failureCode, DateTime nowUtc)
    {
        EnsureCreated();

        if (string.IsNullOrWhiteSpace(failureCode))
            throw new ArgumentException("Failure code is required.");

        Status = PaymentAttemptStatus.Failed;
        FailureCode = failureCode;
        UpdateTimestamp(nowUtc);
    }

    public void MarkExpired(DateTime nowUtc)
    {
        EnsureCreated();

        Status = PaymentAttemptStatus.Expired;
        UpdateTimestamp(nowUtc);
    }

    private void EnsureCreated()
    {
        if (Status != PaymentAttemptStatus.Created)
            throw new InvalidOperationException(
                "Final payment attempt status cannot be changed.");
    }
}