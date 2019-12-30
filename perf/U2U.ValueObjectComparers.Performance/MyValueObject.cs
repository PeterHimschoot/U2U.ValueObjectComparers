using System;
using System.Diagnostics.CodeAnalysis;

namespace U2U.ValueObjectComparers
{
  public sealed class MyNestedValueObject : IEquatable<MyNestedValueObject>
  {
    public decimal Price { get; set; }

    public DateTime When { get; set; }

    public override bool Equals(object obj)
      => ValueObjectComparer<MyNestedValueObject>.Instance.Equals(this, obj);
    public bool Equals([AllowNull] MyNestedValueObject other)
      => ValueObjectComparer<MyNestedValueObject>.Instance.Equals(this, other);
  }

  public sealed class MyValueObject : IEquatable<MyValueObject>
  {

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public MyNestedValueObject Nested { get; set; }
    public override bool Equals(object obj)
      => ValueObjectComparer<MyValueObject>.Instance.Equals(this, obj);
    public bool Equals([AllowNull] MyValueObject other)
      => ValueObjectComparer<MyValueObject>.Instance.Equals(this, other);
  }
}
