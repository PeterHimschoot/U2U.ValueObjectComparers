using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using U2U.ValueObjectComparers;
using U2U.ValueObjectComparers.Tests;
using Xunit;
using Xunit.Abstractions;

#nullable enable

namespace U2U.EntityFrameworkCore.Abstractions.Tests
{
  public class ValueObjectComparerShould
  {
    private readonly ITestOutputHelper output;

    public ValueObjectComparerShould(ITestOutputHelper output)
    {
      this.output = output;
    }

    [Fact]
    public void ReturnTrueForBothNullReferences()
    {
      SomeObject? obj1 = null;
      SomeObject? obj2 = null;
      ValueObjectComparer<SomeObject> sut = ValueObjectComparer<SomeObject>.Instance;
      sut.Equals(obj1, obj2).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForSameReferences()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 }; ;
      obj1.Equals(obj1).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForEitherNullReferences()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      ValueObjectComparer<SomeObject> sut = ValueObjectComparer<SomeObject>.Instance;
      sut.Equals(null, obj1).Should().BeFalse();
      sut.Equals(obj1, null).Should().BeFalse();
    }

    private class SomeObjectWithValueTypeProperty
    {
      public int Age { get; set; }

      public int? Agee { get; set; }

      public override bool Equals(object? obj)
        => ValueObjectComparer<SomeObjectWithValueTypeProperty>.Instance.Equals(this, obj);

      public override int GetHashCode()
       => ValueObjectComparer<SomeObjectWithValueTypeProperty>.Instance.GetHashCode(this);

    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithJustValueTypeProperty()
    {
      var obj1 = new SomeObjectWithValueTypeProperty() { Age = 43, Agee = null };
      var obj2 = new SomeObjectWithValueTypeProperty() { Age = 43, Agee = null };
      obj1.Equals(obj2).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonequalObjectsWithJustValueTypeProperty()
    {
      var obj1 = new SomeObjectWithValueTypeProperty() { Age = 43 };
      var obj2 = new SomeObjectWithValueTypeProperty() { Age = 42 };
      obj1.Equals(obj2).Should().BeFalse();
    }

    private class SomeObjectWithStringProperty
    {
      public string? Name { get; set; }

      public override bool Equals(object? obj)
        => ValueObjectComparer<SomeObjectWithStringProperty>.Instance.Equals(this, obj);
      public override int GetHashCode()
       => ValueObjectComparer<SomeObjectWithStringProperty>.Instance.GetHashCode(this);

    }

    private delegate bool CompFunc<T>(in T x, in T y) where T : struct;

    [Fact]
    public void UseInModifierInLambda()
    {
      CompFunc<int> c = (in int x, in int y) => x == y;

      Expression<Func<string, string, bool>> ce =
        (string x, string y) => object.ReferenceEquals(x, y)
                             || (x != null && x.Equals(y));

    }



    [Fact]
    public void ReturnTrueForEqualObjectsWithJustStringProperty()
    {
      var obj1 = new SomeObjectWithStringProperty() { Name = "Jefke" };
      var obj2 = new SomeObjectWithStringProperty() { Name = "Jefke" };
      obj1.Equals(obj2).Should().BeTrue();
      obj2.Equals(obj1).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonequalObjectsWithJustStringProperty()
    {
      var obj1 = new SomeObjectWithStringProperty() { Name = "Jefke" };
      var obj2 = new SomeObjectWithStringProperty() { Name = "Joske" };
      obj1.Equals(obj2).Should().BeFalse();
      obj2.Equals(obj1).Should().BeFalse();
    }

    [Fact]
    public void ReturnTrueForEqualObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeObject() { Name = "Jef" + "ke", Age = 43 };
      obj1.Equals(obj2).Should().BeTrue();
      obj2.Equals(obj1).Should().BeTrue();
      (obj1 == obj2).Should().BeTrue();
      (obj2 == obj1).Should().BeTrue();
    }

    [Fact]
    public void IgnorePropertiesWithIgnoreAttrbute()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43, NotUsed = 44 };
      var obj2 = new SomeObject() { Name = "Jef" + "ke", Age = 43, NotUsed = 666 };
      obj1.Equals(obj2).Should().BeTrue();
      obj2.Equals(obj1).Should().BeTrue();
      (obj1 == obj2).Should().BeTrue();
      (obj2 == obj1).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForSameStruct()
    {
      var obj1 = new SomeStruct() { Name = "Jefke", Age = 43 };
      obj1.Equals(obj1).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
      (obj1 == obj1).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
    }

    [Fact]
    public void ReturnTrueForEqualStructs()
    {
      var obj1 = new SomeStruct() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeStruct() { Name = "Jef" + "ke", Age = 43 };
      obj1.Equals(obj2).Should().BeTrue();
      obj2.Equals(obj1).Should().BeTrue();
      (obj1 == obj2).Should().BeTrue();
      (obj2 == obj1).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForNonEqualStructs()
    {
      var obj1 = new SomeStruct() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeStruct() { Name = "Jef", Age = 43 };
      obj1.Equals(obj2).Should().BeFalse();
      obj2.Equals(obj1).Should().BeFalse();
      (obj1 == obj2).Should().BeFalse();
      (obj2 == obj1).Should().BeFalse();
      (obj1 != obj2).Should().BeTrue();
      (obj2 != obj1).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithNestedNulls()
    {
      DateTime now = DateTime.Now;
      NestedValueObject? nested1 = null;
      NestedValueObject? nested2 = null;

      var obj1 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested1
      };
      var obj2 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested2
      };
      obj1.Equals(obj2).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithNested()
    {
      DateTime now = DateTime.Now;
      var nested1 = new NestedValueObject { Price = 10M, When = now };
      var nested2 = new NestedValueObject { Price = 10M, When = now };
      nested1.Equals(nested2).Should().BeTrue(because: "The nested objects should equal");

      var obj1 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested1
      };
      var obj2 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested2
      };
      obj1.Equals(obj2).Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForEqualObjectsWithNestedNulls()
    {
      DateTime now = DateTime.Now;
      NestedValueObject? nested1 = null;
      var nested2 = new NestedValueObject { Price = 10M, When = now }; ;

      var obj1 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested1
      };
      var obj2 = new SomeObjectWithNested()
      {
        Name = "Jefke",
        Age = 43,
        Nested = nested2
      };
      obj1.Equals(obj2).Should().BeFalse();
      obj2.Equals(obj1).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseForNonEqualObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeObject() { Name = "Jef", Age = 43 };
      obj1.Equals(obj2).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseForDiffTypedObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeStruct() { Name = "Jefke", Age = 43 };
      obj1.Equals(obj2).Should().BeFalse();
    }

    //private static Func<T, int> GenerateHasher<T>()
    //{
    //  ParameterExpression obj = Expression.Parameter(typeof(T), "obj");
    //  Type hashCodeType = typeof(HashCode);
    //  MethodInfo addMethod = hashCodeType.GetMethods().Single(method => method.Name == "Add" && method.GetParameters().Length == 1);
    //  MethodInfo hashCodeMethod = hashCodeType.GetMethod("ToHashCode", BindingFlags.Public | BindingFlags.Instance)!;
    //  ParameterExpression hashCode = Expression.Variable(hashCodeType, "hashCode");

    //  BlockExpression block = Expression.Block(
    //    type: typeof(int),
    //    variables: new ParameterExpression[] { hashCode },
    //    expressions: new Expression[]
    //    {
    //      Expression.Assign(hashCode, Expression.New(hashCodeType)),
    //      Expression.Block(GenerateAddToHashCodeExpressions()),
    //      Expression.Call(hashCode, hashCodeMethod)
    //    });


    //  Func<T, int> hasher = Expression.Lambda<Func<T, int>>(block, obj).Compile();
    //  return hasher;

    //  Expression[] GenerateAddToHashCodeExpressions()
    //  {
    //    List<Expression> adders = new List<Expression>();
    //    foreach (PropertyInfo propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
    //    {
    //      if (propInfo.IsDefined(typeof(IgnoreAttribute)))
    //      {
    //        continue;
    //      }
    //      MethodInfo boundAddMethod = addMethod.MakeGenericMethod(propInfo.PropertyType);
    //      adders.Add(Expression.Call(hashCode, boundAddMethod, Expression.Property(obj, propInfo)));
    //    }
    //    return adders.ToArray();
    //  }
    //}

    [Fact]
    public void ReturnSameHashCodeForEqualObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeObject() { Name = "Jefke", Age = 43 };

      output.WriteLine($"# obj1 = {obj1.GetHashCode()}");
      output.WriteLine($"# obj2 = {obj2.GetHashCode()}");

      (obj1 == obj2).Should().BeTrue();
      obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    public void ReturnSameHashCodeForSameObject()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };

#pragma warning disable CS1718 // Comparison made to same variable
      (obj1 == obj1).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
      obj1.GetHashCode().Should().Be(obj1.GetHashCode());
    }

    [Fact]
    public void CheckAssumptionsForCollections()
    {
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", "Volleyball" };

      hobbies1.SequenceEqual(hobbies2).Should().BeTrue();
    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithNestedCollections()
    {
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", "Volleyball" };

      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies2
      };

      obj1.Equals(obj2).Should().BeTrue(because: "The objects should equal");
      obj2.Equals(obj1).Should().BeTrue(because: "The objects should equal");
    }

    [Fact]
    public void ReturnSameHashCodeForEqualObjectsWithNestedCollections()
    {
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", "Volleyball" };

      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies2
      };

      output.WriteLine($"# obj1 = {obj1.GetHashCode()}");
      output.WriteLine($"# obj2 = {obj2.GetHashCode()}");

      obj1.Should().Be(obj2);
      obj1.GetHashCode().Should().Be(obj2.GetHashCode(), because: "The objects are equal so they should have the same hashcode.");
    }

    [Fact]
    public void ReturnDifferentHashCodeForObjectsWithDifferentCollections()
    {
      // Hashcode for different objects can be the same, but very rare, so I'll take my chances 
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", "Tennis" };

      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies2
      };

      output.WriteLine($"# obj1 = {obj1.GetHashCode()}");
      output.WriteLine($"# obj2 = {obj2.GetHashCode()}");

      obj1.GetHashCode().Should().NotBe(obj2.GetHashCode(), because: "The objects are not equal so they should not normally have the same hashcode.");
    }

    [Fact]
    public void ReturnSameHashCodeForEqualObjectsWithNestedCollectionsEvenWithNulls()
    {
      var hobbies1 = new List<string> { "WindSurfing", null, "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", null, "Volleyball" };

      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies2
      };

      output.WriteLine($"# obj1 = {obj1.GetHashCode()}");
      output.WriteLine($"# obj2 = {obj2.GetHashCode()}");

      obj1.Should().Be(obj2);
      obj1.GetHashCode().Should().Be(obj2.GetHashCode(), because: "The objects are equal so they should have the same hashcode.");
    }

    [Fact]
    public void ReturnFalseForObjectsWithDifferentNestedCollections()
    {
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var hobbies2 = new List<string> { "WindSurfing", "Tennis" };

      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies2
      };

      obj1.Equals(obj2).Should().BeFalse(because: "The objects should equal");
      obj2.Equals(obj1).Should().BeFalse(because: "The objects should equal");
    }

    [Fact]
    public void ReturnDifferentHashCodeForNonEqualObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeObject() { Name = "Jef", Age = 43 };

      output.WriteLine($"# obj1 = {obj1.GetHashCode()}");
      output.WriteLine($"# obj2 = {obj2.GetHashCode()}");

      obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
    }

    //[Fact]
    //public void CheckHashCodeClass()
    //{
    //  var hashCode = new HashCode();
    //  hashCode.Add("Jefke");
    //  hashCode.Add(43);
    //  var firstHash = hashCode.ToHashCode();

    //  hashCode = new HashCode();
    //  hashCode.Add("Jefke");
    //  hashCode.Add(43);
    //  var secondHash = hashCode.ToHashCode();

    //  hashCode = new HashCode();
    //  hashCode.Add("Jef");
    //  hashCode.Add(43);
    //  var thirdHash = hashCode.ToHashCode();

    //  output.WriteLine($"{firstHash} - {secondHash} - {thirdHash}");
    //}

    [Fact]
    public void ReturnTrueForEqualObjectsWithNullCollections()
    {
      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = null
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = null
      };

      obj1.Equals(obj2).Should().BeTrue(because: "The objects should equal");
      obj2.Equals(obj1).Should().BeTrue(because: "The objects should equal");
    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithSameCollections()
    {
      var hobbies1 = new List<string> { "WindSurfing", "Volleyball" };
      var obj1 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };
      var obj2 = new SomeObjectWithCollection
      {
        Name = "Jefke",
        Age = 43,
        Hobbies = hobbies1
      };

      obj1.Equals(obj2).Should().BeTrue(because: "The objects should equal");
      obj2.Equals(obj1).Should().BeTrue(because: "The objects should equal");
    }

    [Fact]
    public void ReturnTrueForEqualObjectsWithNestedValueObjectCollection()
    {
      DateTime someDate = new DateTime(2020, 03, 31);
      var obj1 = new SomeObjectWithNestedCollection
      {
        Name = "Jefke",
        Age = 43,
        Nested = new NestedValueObject[]
        {
          new NestedValueObject { Price = 100, When = someDate },
          new NestedValueObject { Price = 200, When = someDate.AddDays(1) },
          new NestedValueObject { Price = 300, When = someDate.AddDays(2) }
       }
      };
      var obj2 = new SomeObjectWithNestedCollection
      {
        Name = "Jefke",
        Age = 43,
        Nested = new NestedValueObject[]
        {
          new NestedValueObject { Price = 100, When = someDate },
          new NestedValueObject { Price = 200, When = someDate.AddDays(1) },
          new NestedValueObject { Price = 300, When = someDate.AddDays(2) }
        }
      };
      obj1.Should().Be(obj2);
    }

    [Fact]
    public void ReturnSameHashCodeForEqualObjectsWithNestedValueObjectCollection()
    {
      DateTime someDate = new DateTime(2020, 03, 31);
      var obj1 = new SomeObjectWithNestedCollection
      {
        Name = "Jefke",
        Age = 43,
        Nested = new NestedValueObject[]
        {
          new NestedValueObject { Price = 100, When = someDate },
          new NestedValueObject { Price = 200, When = someDate.AddDays(1) },
          new NestedValueObject { Price = 300, When = someDate.AddDays(2) }
       }
      };
      var obj2 = new SomeObjectWithNestedCollection
      {
        Name = "Jefke",
        Age = 43,
        Nested = new NestedValueObject[]
        {
          new NestedValueObject { Price = 100, When = someDate },
          new NestedValueObject { Price = 200, When = someDate.AddDays(1) },
          new NestedValueObject { Price = 300, When = someDate.AddDays(2) }
        }
      };

      obj1.Should().Be(obj2);
      obj1.GetHashCode().Should().Be(obj2.GetHashCode(), because: "The objects are equal so they should have the same hashcode.");
    }
  }
}
