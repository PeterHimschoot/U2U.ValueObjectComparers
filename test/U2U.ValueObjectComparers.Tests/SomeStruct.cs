using System;
using System.Diagnostics.CodeAnalysis;
using U2U.ValueObjectComparers;

namespace U2U.EntityFrameworkCore.Abstractions.Tests
{
  public class SomeStruct : IEquatable<SomeStruct>
  {
    public static bool operator ==(SomeStruct left, SomeStruct right)
     => left.Equals(right);

    public static bool operator !=(SomeStruct left, SomeStruct right)
      => !(left == right);

    public string Name { get; set; }

    public int Age { get; set; }

    public override bool Equals(object obj)
      => ValueObjectComparer<SomeStruct>.Instance.Equals(this, obj);

    public bool Equals([AllowNull]SomeStruct other)
      => ValueObjectComparer<SomeStruct>.Instance.Equals(this, other);

    public override int GetHashCode()
      => ValueObjectComparer<SomeStruct>.Instance.GetHashCode(this);

  }
}
