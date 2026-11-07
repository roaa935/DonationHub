namespace DonationHub.Domain.Entities;

public class ProcessedStripeEvent
{
    public string EventId { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedStripeEvent() { }

    private ProcessedStripeEvent(
        string eventId,
        string type,
        DateTime processedAtUtc)
    {
        EventId = eventId;
        Type = type;
        ProcessedAt = processedAtUtc;
    }

    public static ProcessedStripeEvent Create(
        string eventId,
        string type,
        DateTime processedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("Event ID is required.");

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Event type is required.");

        return new ProcessedStripeEvent(
            eventId, type, processedAtUtc);
    }
}