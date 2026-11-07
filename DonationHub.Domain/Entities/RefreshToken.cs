using DonationHub.Domain.Common;

namespace DonationHub.Domain.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    // navigation properties
    public ApplicationUser User { get; private set; } = null!;
    private RefreshToken() { }

    private RefreshToken(Guid userId,string tokenHash,DateTime createdAtUtc,DateTime expiresAtUtc): base(Guid.NewGuid(), createdAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAtUtc;
    }

    public static RefreshToken Create(Guid userId,string tokenHash,DateTime createdAtUtc,DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash is required.");

        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("Expiration must be after creation.");

        return new RefreshToken(
            userId, tokenHash, createdAtUtc, expiresAtUtc);
    }

    public bool IsActive(DateTime nowUtc)
        
    {
        // the logic here is that a token is considered active if it has not been revoked and has not expired yet wich i do not get it and i replaced it with updated at the on in the entity how can we make the new logic 
        return UpdatedAt == null && ExpiresAt > nowUtc;
    }

    public void Revoke(DateTime nowUtc, Guid? replacedByTokenId = null)
    {
        if (!IsActive(nowUtc))
            throw new InvalidOperationException("Token is not active.");

        if (replacedByTokenId == Id)
            throw new ArgumentException("A token cannot replace itself.");
        UpdateTimestamp(nowUtc);

        ReplacedByTokenId = replacedByTokenId;
    }
}