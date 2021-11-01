namespace U2U.ValueObjectComparers.Tests;

public class SomeObjectWithNested
{
  public static bool operator ==(SomeObjectWithNested left, SomeObjectWithNested right)
  {
    if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
    {
      return false;
    }
    return ReferenceEquals(left, null) || left.Equals(right);
  }

  public static bool operator !=(SomeObjectWithNested left, SomeObjectWithNested right)
    => !(left == right);

  public string Name { get; set; }

  public int Age { get; set; }

  public NestedValueObject Nested { get; set; }

  public override bool Equals(object obj)
    => ValueObjectComparer<SomeObjectWithNested>.Instance.Equals(this, obj);

  public override int GetHashCode()
    => ValueObjectComparer<SomeObjectWithNested>.Instance.GetHashCode(this);
}
