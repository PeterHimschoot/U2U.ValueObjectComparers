using System;
using System.Diagnostics.CodeAnalysis;

namespace U2U.ValueObjectComparers
{
  public sealed class HCNestedValueObject : IEquatable<HCNestedValueObject>
  {
    public static bool operator==(HCNestedValueObject left, HCNestedValueObject right)
    {
      if (object.ReferenceEquals(left, right))
      {
        return true;
      }
      return (left is object) && left.Equals(right);
    }

    public static bool operator!=(HCNestedValueObject left, HCNestedValueObject right)
    => !(left == right);

    public decimal Price { get; set; }

    public DateTime When { get; set; }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj))
      {
        return true;
      }
      if (this.GetType() == obj?.GetType())
      {
        var other = obj as HCNestedValueObject;
        return Equals(other);
      }
      return false;
    }

    public bool Equals([AllowNull] HCNestedValueObject other)
      => this.Price == other.Price && this.When == other.When;

    public override int GetHashCode()
    {
      var hash = new HashCode();
      hash.Add(this.Price);
      hash.Add(this.When);
      return hash.ToHashCode();
    }
  //=> HashCode.Combine(this.Price, this.When);

  }

  public sealed class HCValueObject : IEquatable<HCValueObject>
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public HCNestedValueObject Nested { get; set; }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj))
      {
        return true;
      }
      if (this.GetType() == obj?.GetType())
      {
        var other = obj as HCValueObject;
        return Equals(other);
      }
      return false;
    }

    public bool Equals([AllowNull] HCValueObject other) 
      =>  object.ReferenceEquals(this, other) 
      || (this.FirstName == other.FirstName
          && this.LastName == other.LastName
          && this.Age == other.Age
          && this.Nested == other.Nested);

    public override int GetHashCode()
      => HashCode.Combine(this.FirstName, this.LastName, this.Age, this.Nested);
  }

}
