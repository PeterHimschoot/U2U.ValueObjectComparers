using System;

namespace U2U.ValueObjectComparers
{
  public class HCNestedValueObject
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
        return this.Price == other.Price && this.When == other.When;
      }
      return false;
    }
  }

  public class HCValueObject
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
        return this.FirstName == other.FirstName
          && this.LastName == other.LastName
          && this.Age == other.Age
          && this.Nested == other.Nested;
      }
      return false;
    }
  }

}
