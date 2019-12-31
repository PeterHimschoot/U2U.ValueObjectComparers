using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace U2U.ValueObjectComparers
{
  public struct MyValueObjectStruct : IEquatable<MyValueObjectStruct>
  {
    public static bool operator==(in MyValueObjectStruct left, in MyValueObjectStruct right)
    => ValueObjectComparerStruct<MyValueObjectStruct>.Instance.Equals(left, right);

    public static bool operator !=(in MyValueObjectStruct left, in MyValueObjectStruct right)
      => !(left == right);

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public MyNestedValueObject Nested { get; set; }

    public override bool Equals(object obj)
      => ValueObjectComparerStruct<MyValueObjectStruct>.Instance.Equals(this, obj);
    public bool Equals(MyValueObjectStruct other)
      => ValueObjectComparerStruct<MyValueObjectStruct>.Instance.Equals(this, other);
    public override int GetHashCode()
      => ValueObjectComparerStruct<MyValueObjectStruct>.Instance.GetHashCode();

  }
}
