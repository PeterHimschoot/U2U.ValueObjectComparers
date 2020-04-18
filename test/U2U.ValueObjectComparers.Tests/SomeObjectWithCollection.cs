using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using U2U.ValueObjectComparers;

namespace U2U.EntityFrameworkCore.Abstractions.Tests
{
  public class SomeObjectWithCollection : IEquatable<SomeObject>
  {
    public static bool operator ==(SomeObjectWithCollection left, SomeObjectWithCollection right)
      => ValueObjectComparer<SomeObjectWithCollection>.Instance.Equals(left, right);

    public static bool operator !=(SomeObjectWithCollection left, SomeObjectWithCollection right)
      => !(left == right);

    public string Name { get; set; }

    public int Age { get; set; }

    [DeepCompare]
    public List<string> Hobbies { get; set; }

    public override bool Equals([AllowNull] object obj)
      => ValueObjectComparer<SomeObjectWithCollection>.Instance.Equals(this, obj);

    public bool Equals([AllowNull] SomeObject other)
      => ValueObjectComparer<SomeObjectWithCollection>.Instance.Equals(this, other);

    public override int GetHashCode()
      => ValueObjectComparer<SomeObjectWithCollection>.Instance.GetHashCode(this);
  }
}
