using System;
using U2U.ValueObjectComparers;

namespace U2U.EntityFrameworkCore.Abstractions.Tests
{
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

    public override bool Equals(object obj)
      => ValueObjectComparer<NestedValueObject>.Instance.Equals(this, obj);
  }

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
  }
}