namespace DonationHub.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public string Currency { get; }

    private Money() { }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount must be positive.");

        if (string.IsNullOrWhiteSpace(currency) ||
            currency.Length != 3 ||
            !currency.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                "Currency must be a 3-letter code.");
        }

        return new Money(
            amount,
            currency.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Currencies must match.");

        return Money.Create(Amount + other.Amount, Currency);
    }
}