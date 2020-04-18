using System.Runtime.CompilerServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

[assembly: InternalsVisibleTo("U2U.ValueObjectComparers.Tests")]

namespace U2U.ValueObjectComparers
{
  internal delegate bool CompFunc<T>(T left, T right);

  /// <summary>
  /// Helper class to take care of generating the comparer expression using "Just once" reflection.
  /// </summary>
  internal static class ExpressionGenerater
  {
    internal static MethodInfo SequenceEqualMethod;
    internal static MethodInfo SequenceHashCodeMethod;
    internal static MethodInfo AddHashCodeMethod;
    internal static MethodInfo ToHashCodeMethod;
    internal static MethodInfo AddCollectionHashCodeMethod;

    static ExpressionGenerater()
    {
      SequenceEqualMethod = typeof(Enumerable)
       .GetMethods(bindingAttr: BindingFlags.Public | BindingFlags.Static)
       .Single(methodInfo => methodInfo.Name == nameof(Enumerable.SequenceEqual) && methodInfo.GetParameters().Length == 2);

      Type hashCodeType = typeof(HashCode);
      AddHashCodeMethod = hashCodeType.GetMethods()
        .Single(method => method.Name == nameof(HashCode.Add) && method.GetParameters().Length == 1);
      AddCollectionHashCodeMethod = AddHashCodeMethod.MakeGenericMethod(typeof(int));
      ToHashCodeMethod =
        hashCodeType.GetMethod(nameof(HashCode.ToHashCode), BindingFlags.Public | BindingFlags.Instance)!;

      SequenceHashCodeMethod = typeof(ExpressionGenerater)
        .GetMethods(bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
        .Single(methodInfo => methodInfo.Name == nameof(ExpressionGenerater.AddHashCodeMembersForCollection));


    }

    private static Expression GenerateEqualityExpression(ParameterExpression left, ParameterExpression right, PropertyInfo propInfo)
    {
      Type propertyType = propInfo.PropertyType;
      Type equitableType = typeof(IEquatable<>).MakeGenericType(propertyType);

      MethodInfo equalMethod;
      Expression equalCall;
      if (equitableType.IsAssignableFrom(propertyType))
      {
        equalMethod = equitableType.GetMethod(nameof(Equals), new Type[] { propertyType });
        equalCall = Expression.Call(Expression.Property(left, propInfo), equalMethod, Expression.Property(right, propInfo));
      }
      else if (propInfo.IsDefined(typeof(DeepCompareAttribute)))
      {
        // Get type of collection elements
        var collectionElementType = propertyType.GetEnumeratedType();
        var boundEqualMethod = SequenceEqualMethod.MakeGenericMethod(collectionElementType);
        var asEnumerableType = typeof(IEnumerable<>).MakeGenericType(collectionElementType);
        var leftCast = Expression.Convert(Expression.Property(left, propInfo), asEnumerableType);
        var rightCast = Expression.Convert(Expression.Property(right, propInfo), asEnumerableType);
        equalCall = Expression.Call(instance: null, method: boundEqualMethod, arg0: leftCast, arg1: rightCast);
      }
      else
      {
        equalMethod = propertyType.GetMethod(nameof(Equals), new Type[] { typeof(object) });
        equalCall = Expression.Call(Expression.Property(left, propInfo), equalMethod, Expression.Convert(Expression.Property(right, propInfo), typeof(object)));
      }

      if (propInfo.PropertyType.IsValueType)
      {
        // Property is value type, no need to check for null, so directly call Equals
        return equalCall;
      }
      else
      {
        // Generate
        //       Expression<Func<T, T, bool>> ce = (T x, T y) => object.ReferenceEquals(x, y) || (x != null && x.Equals(y));

        Expression leftValue = Expression.Property(left, propInfo);
        Expression rightValue = Expression.Property(right, propInfo);
        Expression refEqual = Expression.ReferenceEqual(leftValue, rightValue);
        Expression nullConst = Expression.Constant(null);
        Expression leftIsNotNull = Expression.Not(Expression.ReferenceEqual(leftValue, nullConst));
        Expression leftIsNotNullAndIsEqual = Expression.AndAlso(leftIsNotNull, equalCall);
        Expression either = Expression.OrElse(refEqual, leftIsNotNullAndIsEqual);

        return either;
      }
    }

    internal static CompFunc<T> GenerateComparer<T>()
    {
      List<Expression> comparers = new List<Expression>();
      ParameterExpression left = Expression.Parameter(typeof(T), "left");
      ParameterExpression right = Expression.Parameter(typeof(T), "right");

      foreach (PropertyInfo propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
      {
        if (propInfo.IsDefined(typeof(IgnoreAttribute)))
        {
          continue;
        }
        else
        {
          comparers.Add(GenerateEqualityExpression(left, right, propInfo));
        }
      }
      Expression ands = comparers.Aggregate((left, right) => Expression.AndAlso(left, right));
      var andComparer = Expression.Lambda<CompFunc<T>>(ands, left, right).Compile();
      return andComparer;
    }

    internal static int AddHashCodeMembersForCollection<T>(IEnumerable<T> coll)
    {
      HashCode hashCode = new HashCode();
      if (coll != null)
      {
        foreach (T el in coll)
        {
          hashCode.Add(el != null ? el.GetHashCode() : 0);
        }
      }
      return hashCode.ToHashCode();
    }

    internal static Func<T, int> GenerateHasher<T>()
    {
      // Generates the equivalent of
      // var hash = new HashCode();
      // hash.Add(this.Price);
      // hash.Add(this.When);
      // AddHasCodeMembersForCollection(hash, this.Hobbies) where this.Hobbies has DeepCompare attribute.
      // return hash.ToHashCode();
      ParameterExpression obj = Expression.Parameter(typeof(T), "obj");
      ParameterExpression hashCode = Expression.Variable(typeof(HashCode), "hashCode");

      List<Expression> parts = GenerateAddToHashCodeExpressions();
      parts.Insert(0, Expression.Assign(hashCode, Expression.New(typeof(HashCode))));
      parts.Add(Expression.Call(hashCode, ToHashCodeMethod));
      Expression[] body = parts.ToArray();

      BlockExpression block = Expression.Block(
        type: typeof(int),
        variables: new ParameterExpression[] { hashCode },
        expressions: body);

      Func<T, int> hasher = Expression.Lambda<Func<T, int>>(block, obj).Compile();
      return hasher;

      List<Expression> GenerateAddToHashCodeExpressions()
      {
        List<Expression> adders = new List<Expression>();
        foreach (PropertyInfo propInfo in typeof(T)
          .GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
        {
          if (propInfo.IsDefined(typeof(IgnoreAttribute)))
          {
            continue;
          }
          if (propInfo.IsDefined(typeof(DeepCompareAttribute)))
          {
            Type? collectionElementType = propInfo.PropertyType.GetEnumeratedType();
            var sequenceHashCode = SequenceHashCodeMethod.MakeGenericMethod(collectionElementType);
            var asEnumerableType = typeof(IEnumerable<>).MakeGenericType(collectionElementType);
            var cast = Expression.Convert(Expression.Property(obj, propInfo), asEnumerableType);
            var call = Expression.Call(instance: null, method: sequenceHashCode, arguments: cast);
            adders.Add(Expression.Call(instance: hashCode, AddCollectionHashCodeMethod, call));
          }
          else
          {
            MethodInfo boundAddMethod = AddHashCodeMethod.MakeGenericMethod(propInfo.PropertyType);
            adders.Add(Expression.Call(instance: hashCode, boundAddMethod, Expression.Property(obj, propInfo)));
          }
        }
        return adders;
      }
    }
  }

  public sealed class ValueObjectComparer<T> where T : class
  {
    public static ValueObjectComparer<T> Instance { get; } = new ValueObjectComparer<T>();

    private CompFunc<T> comparer = ExpressionGenerater.GenerateComparer<T>();
    private Func<T, int> hasher = ExpressionGenerater.GenerateHasher<T>();

    public bool Equals(T? left, T? right)
      => object.ReferenceEquals(left, right)
      || (left is object && right is object && this.comparer(left, right));

    public bool Equals(T? left, object? right)
      => object.ReferenceEquals(left, right)
      || (right is object && right.GetType() == left?.GetType() && this.comparer(left, (T)right));

    public int GetHashCode(T obj)
      => hasher(obj);

    public override int GetHashCode()
      => throw new InvalidOperationException("Do not call GetHashCode(), instead use GetHashCode(this)");
  }

  public sealed class ValueObjectComparerStruct<T> where T : struct
  {
    public static ValueObjectComparerStruct<T> Instance { get; } = new ValueObjectComparerStruct<T>();

    private CompFunc<T> comparer = ExpressionGenerater.GenerateComparer<T>();
    private Func<T, int> hasher = ExpressionGenerater.GenerateHasher<T>();

    public bool Equals(in T left, in T right)
      => this.comparer(left, right);

    public bool Equals(T left, object right)
      => right is object && right.GetType() == typeof(T) && Equals(left, (T)right);

    public int GetHashCode(in T obj)
      => hasher(obj);

    public override int GetHashCode()
  => throw new InvalidOperationException("Do not call GetHashCode(), instead use GetHashCode(this)");

  }
}
