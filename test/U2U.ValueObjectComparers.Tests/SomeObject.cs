﻿namespace U2U.ValueObjectComparers.Tests;

public class SomeObject : IEquatable<SomeObject>
{
  public static bool operator ==(SomeObject left, SomeObject right)
    => ValueObjectComparer<SomeObject>.Instance.Equals(left, right);

  public static bool operator !=(SomeObject left, SomeObject right)
    => !(left == right);

  public string Name { get; set; }

  public int Age { get; set; }

  [Ignore]
  public int NotUsed { get; set; }

  public override bool Equals([AllowNull] object obj)
    => ValueObjectComparer<SomeObject>.Instance.Equals(this, obj);

  public bool Equals([AllowNull] SomeObject other)
    => ValueObjectComparer<SomeObject>.Instance.Equals(this, other);

  public override int GetHashCode()
    => ValueObjectComparer<SomeObject>.Instance.GetHashCode(this);
}
