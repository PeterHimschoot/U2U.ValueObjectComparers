using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace U2U.ValueObjectComparers
{
  public abstract class ValueObject
  {
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
      if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
      {
        return false;
      }
      return ReferenceEquals(left, null) || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
      => !(EqualOperator(left, right));

    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object obj)
    {
      if( object.ReferenceEquals(this, obj))
      {
        return true;
      }
      if (obj == null || obj.GetType() != GetType())
      {
        return false;
      }

      ValueObject other = (ValueObject)obj;
      IEnumerator<object> thisValues = GetAtomicValues().GetEnumerator();
      IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();
      while (thisValues.MoveNext() && otherValues.MoveNext())
      {
        if (ReferenceEquals(thisValues.Current, null) ^
            ReferenceEquals(otherValues.Current, null))
        {
          return false;
        }

        if (thisValues.Current != null &&
            !thisValues.Current.Equals(otherValues.Current))
        {
          return false;
        }
      }
      return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    public override int GetHashCode()
    {
      return GetAtomicValues()
       .Select(x => x != null ? x.GetHashCode() : 0)
       .Aggregate((x, y) => x ^ y);
    }
  }

  public sealed class MSNestedValueObject : ValueObject, IEquatable<MSNestedValueObject>
  {
    public decimal Price { get; set; }

    public DateTime When { get; set; }

    public bool Equals([AllowNull] MSNestedValueObject other)
      => base.Equals(other);

    protected override IEnumerable<object> GetAtomicValues()
    {
      yield return Price;
      yield return When;
    }
  }
  public sealed class MSValueObject : ValueObject, IEquatable<MSValueObject>
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public MSNestedValueObject Nested { get; set; }

    public bool Equals([AllowNull] MSValueObject other) 
      => base.Equals(other);

    protected override IEnumerable<object> GetAtomicValues()
    {
      yield return this.FirstName;
      yield return this.LastName;
      yield return this.Age;
      yield return this.Nested;
    }
  }
}
