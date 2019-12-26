using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchMarkSkeleton
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

  public class MSNestedValueObject : ValueObject
  {
    public decimal Price { get; set; }

    public DateTime When { get; set; }

    protected override IEnumerable<object> GetAtomicValues()
    {
      yield return Price;
      yield return When;
    }
  }
  public class MSValueObject : ValueObject
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public MSNestedValueObject Nested { get; set; }

    protected override IEnumerable<object> GetAtomicValues()
    {
      yield return this.FirstName;
      yield return this.LastName;
      yield return this.Age;
      yield return this.Nested;
    }
  }
}
