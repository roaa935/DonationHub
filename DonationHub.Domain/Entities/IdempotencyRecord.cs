namespace DonationHub.Domain.Entities;

public class IdempotencyRecord
{
    public string Key { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public string RequestHash { get; private set; } = null!;

    public int ResponseStatusCode { get; private set; }

    public string ResponseBody { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    private IdempotencyRecord() { }

    private IdempotencyRecord(
        string key,
        Guid userId,
        string requestHash,
        int responseStatusCode,
        string responseBody,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        Key = key;
        UserId = userId;
        RequestHash = requestHash;
        ResponseStatusCode = responseStatusCode;
        ResponseBody = responseBody;
        CreatedAt = createdAtUtc;
        ExpiresAt = expiresAtUtc;
    }

    public static IdempotencyRecord Create(
        string key,
        Guid userId,
        string requestHash,
        int responseStatusCode,
        string responseBody,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(requestHash))
            throw new ArgumentException("Request hash is required.");

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("Expiration must be after creation.");

        if (responseStatusCode < 100 || responseStatusCode > 599)
            throw new ArgumentException("Invalid HTTP status code.");

        return new IdempotencyRecord(
            key,
            userId,
            requestHash,
            responseStatusCode,
            responseBody,
            createdAtUtc,
            expiresAtUtc);
    }

    public bool IsExpired(DateTime nowUtc)
    {
        return nowUtc >= ExpiresAt;
    }
}