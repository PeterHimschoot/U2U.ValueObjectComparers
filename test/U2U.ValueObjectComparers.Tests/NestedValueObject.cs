namespace U2U.ValueObjectComparers.Tests;

public class NestedValueObject
{
  public static bool operator ==(NestedValueObject left, NestedValueObject right)
  {
    if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
    {
      return false;
    }
    return ReferenceEquals(left, null) || left.Equals(right);
  }

  public static bool operator !=(NestedValueObject left, NestedValueObject right)
    => !(left == right);

  public decimal Price { get; set; }

  public DateTime When { get; set; }

  public override bool Equals(object? obj)
    => ValueObjectComparer<NestedValueObject>.Instance.Equals(this, obj);

  public override int GetHashCode()
    => ValueObjectComparer<NestedValueObject>.Instance.GetHashCode(this);
}

