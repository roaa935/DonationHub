using System.Collections.Generic;
using System.Linq;

namespace DonationHub.Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    public override int GetHashCode()
    {
        return GetEqualityComponents().Aggregate(0,(current, component) =>HashCode.Combine(current, component));
    }

    public static bool operator ==(ValueObject? left,ValueObject? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=( ValueObject? left,ValueObject? right)
    {
        return !(left == right);
    }
}