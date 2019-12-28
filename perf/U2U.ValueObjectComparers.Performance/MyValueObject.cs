using System;
using System.Diagnostics.CodeAnalysis;
using U2U.ValueObjectComparers;

namespace U2U.ValueObjectComparers
{
  public class NestedValueObject
  {
    public decimal Price { get; set; }

    public DateTime When { get; set; }

    public override bool Equals(object obj)
      => ValueObjectComparer<NestedValueObject>.Instance.Equals(this, obj);
  }

  public class MyValueObject
  {

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public NestedValueObject Nested { get; set; }
    public override bool Equals(object obj)
      => ValueObjectComparer<MyValueObject>.Instance.Equals(this, obj);
  }

  public struct MyValueObjectStruct : IEquatable<MyValueObjectStruct>
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public override bool Equals(object obj)
      => ValueObjectComparer<MyValueObjectStruct>.Instance.Equals(this, obj);
    public bool Equals([AllowNull] MyValueObjectStruct other)
      => ValueObjectComparer<MyValueObjectStruct>.Instance.Equals(this, other);
  }

}
