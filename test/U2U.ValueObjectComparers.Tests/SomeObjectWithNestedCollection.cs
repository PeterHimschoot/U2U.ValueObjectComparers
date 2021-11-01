namespace U2U.ValueObjectComparers.Tests;

public class SomeObjectWithNestedCollection
{
  public static bool operator ==(SomeObjectWithNestedCollection left, SomeObjectWithNestedCollection right)
  {
    if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
    {
      return false;
    }
    return ReferenceEquals(left, null) || left.Equals(right);
  }

  public static bool operator !=(SomeObjectWithNestedCollection left, SomeObjectWithNestedCollection right)
    => !(left == right);

  public string Name { get; set; }

  public int Age { get; set; }

  [DeepCompare]
  public NestedValueObject[] Nested { get; set; }

  public override bool Equals(object obj)
    => ValueObjectComparer<SomeObjectWithNestedCollection>.Instance.Equals(this, obj);

  public override int GetHashCode()
    => ValueObjectComparer<SomeObjectWithNestedCollection>.Instance.GetHashCode(this);
}
