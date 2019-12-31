﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using U2U.ValueObjectComparers;
using Xunit;

#nullable enable

namespace U2U.EntityFrameworkCore.Abstractions.Tests
{
  public class ValueObjectComparerShould
  {
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
       => ValueObjectComparer<SomeObjectWithValueTypeProperty>.Instance.GetHashCode();

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
       => ValueObjectComparer<SomeObjectWithStringProperty>.Instance.GetHashCode();

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
    public void FactCheck()
    {
      var obj1 = new SomeObjectWithStringProperty() { Name = "Jefke" };
      var obj2 = new SomeObjectWithStringProperty() { Name = "Jefke" };
      string altName = "Jef";
      altName += "ke";
      object.ReferenceEquals(obj1.Name, altName).Should().BeFalse();

      PropertyInfo propInfo = typeof(SomeObjectWithStringProperty).GetProperty(nameof(SomeObjectWithStringProperty.Name), BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)!;
      propInfo.PropertyType.Should().BeSameAs(typeof(string));

      ParameterExpression left = Expression.Parameter(typeof(SomeObjectWithStringProperty), "left");
      ParameterExpression right = Expression.Parameter(typeof(SomeObjectWithStringProperty), "right");

      MethodInfo equalMethod = propInfo.PropertyType.GetMethod(nameof(Equals), new Type[] { propInfo.PropertyType })!;
      Expression equalCall = Expression.Call(Expression.Property(left, propInfo), equalMethod, Expression.Property(right, propInfo));

      Func<SomeObjectWithStringProperty, SomeObjectWithStringProperty, bool> comparer = Expression.Lambda<Func<SomeObjectWithStringProperty, SomeObjectWithStringProperty, bool>>(equalCall, left, right).Compile();
      comparer(obj1, obj2).Should().BeTrue();

      Expression leftValue = Expression.Property(left, propInfo);
      Expression rightValue = Expression.Property(right, propInfo);

      Expression refEqual = Expression.ReferenceEqual(leftValue, rightValue);
      comparer = Expression.Lambda<Func<SomeObjectWithStringProperty, SomeObjectWithStringProperty, bool>>(refEqual, left, right).Compile();
      comparer(obj1, obj2).Should().BeTrue();
      obj2.Name = altName;
      comparer(obj1, obj2).Should().BeFalse();

      Expression nullConst = Expression.Constant(null);
      Expression leftIsNotNull = Expression.Not(Expression.ReferenceEqual(leftValue, nullConst));
      Expression rightIsNotNull = Expression.Not(Expression.ReferenceEqual(rightValue, nullConst));
      Expression neitherIsNull = Expression.AndAlso(leftIsNotNull, rightIsNotNull);

      comparer = Expression.Lambda<Func<SomeObjectWithStringProperty, SomeObjectWithStringProperty, bool>>(neitherIsNull, left, right).Compile();
      comparer(obj1, obj2).Should().BeTrue();

      Expression neitherIsNullAndIsEqual = Expression.AndAlso(neitherIsNull, equalCall);
      comparer(obj1, obj2).Should().BeTrue();

      Expression either = Expression.OrElse(refEqual, neitherIsNullAndIsEqual);
      comparer = Expression.Lambda<Func<SomeObjectWithStringProperty, SomeObjectWithStringProperty, bool>>(either, left, right).Compile();
      comparer(obj1, obj2).Should().BeTrue(); // Should call equals
      obj2.Name = "Jefke";
      comparer(obj1, obj2).Should().BeTrue(); // ReferenceEquals
      obj2.Name = null;
      comparer(obj1, obj2).Should().BeFalse(); // One side is null
      comparer(obj2, obj1).Should().BeFalse(); // One side is null
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

    private static Func<T, int> GenerateHasher<T>()
    {
      ParameterExpression obj = Expression.Parameter(typeof(T), "obj");
      Type hashCodeType = typeof(HashCode);
      MethodInfo addMethod = hashCodeType.GetMethods().Single(method => method.Name == "Add" && method.GetParameters().Length == 1);
      MethodInfo hashCodeMethod = hashCodeType.GetMethod("ToHashCode", BindingFlags.Public | BindingFlags.Instance)!;
      ParameterExpression hashCode = Expression.Variable(hashCodeType, "hashCode");

      BlockExpression block = Expression.Block(
        type: typeof(int),
        variables: new ParameterExpression[] { hashCode },
        expressions: new Expression[]
        {
          Expression.Assign(hashCode, Expression.New(hashCodeType)),
          Expression.Block(GenerateAddToHashCodeExpressions()),
          Expression.Call(hashCode, hashCodeMethod)
        });


      Func<T, int> hasher = Expression.Lambda<Func<T, int>>(block, obj).Compile();
      return hasher;

      Expression[] GenerateAddToHashCodeExpressions()
      {
        List<Expression> adders = new List<Expression>();
        foreach (PropertyInfo propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
        {
          if (propInfo.IsDefined(typeof(IgnoreAttribute)))
          {
            continue;
          }
          MethodInfo boundAddMethod = addMethod.MakeGenericMethod(propInfo.PropertyType);
          adders.Add(Expression.Call(hashCode, boundAddMethod, Expression.Property(obj, propInfo)));
        }
        return adders.ToArray();
      }
    }

    [Fact]
    public void ReturnSomeHashCodeForEqualObjects()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };
      var obj2 = new SomeObject() { Name = "Jefke", Age = 43 };

      (obj1 == obj2).Should().BeTrue();
      obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    public void ReturnSomeHashCodeForSameObject()
    {
      var obj1 = new SomeObject() { Name = "Jefke", Age = 43 };

#pragma warning disable CS1718 // Comparison made to same variable
      (obj1 == obj1).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable
      obj1.GetHashCode().Should().Be(obj1.GetHashCode());
    }
  }
}
