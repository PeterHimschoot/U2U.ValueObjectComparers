using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable

namespace U2U.ValueObjectComparers
{
  // Declaring our own delegate to use C# 7 in arguments for struct types, as soon as I figure out how to make this work with Lambda<>.Compile

  internal delegate bool CompFunc<T>(T left, T right);

  /// <summary>
  /// Helper class to take care of generating the comparer expression using "Just once" reflection.
  /// </summary>
  internal static class ExpressionGenerater
  {
    private static Expression GenerateEqualityExpression(ParameterExpression left, ParameterExpression right, PropertyInfo prop)
    {
      Type propertyType = prop.PropertyType;
      Type equitableType = typeof(IEquatable<>).MakeGenericType(propertyType);

      MethodInfo equalMethod;
      Expression equalCall;
      if (equitableType.IsAssignableFrom(propertyType))
      {
        equalMethod = equitableType.GetMethod(nameof(Equals), new Type[] { propertyType });
        equalCall = Expression.Call(Expression.Property(left, prop), equalMethod, Expression.Property(right, prop));
      }
      else
      {
        equalMethod = propertyType.GetMethod(nameof(Equals), new Type[] { typeof(object) });
        equalCall = Expression.Call(Expression.Property(left, prop), equalMethod, Expression.Convert(Expression.Property(right, prop), typeof(object)));
      }

      if (prop.PropertyType.IsValueType)
      {
        // Property if value type so directly call Equals
        return equalCall;
      }
      else
      {
        // Generate
        //       Expression<Func<T, T, bool>> ce = (T x, T y) => object.ReferenceEquals(x, y) || (x != null && x.Equals(y));

        Expression leftValue = Expression.Property(left, prop);
        Expression rightValue = Expression.Property(right, prop);
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
        comparers.Add(GenerateEqualityExpression(left, right, propInfo));
      }
      Expression ands = comparers.Aggregate((left, right) => Expression.AndAlso(left, right));
      var andComparer = Expression.Lambda<CompFunc<T>>(ands, left, right).Compile();
      return andComparer;
    }

    internal static Func<T, int> GenerateHasher<T>()
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

    //private static Expression GeneratePropertyExpression(ParameterExpression obj, PropertyInfo prop)
    //=> Expression.Property(obj, prop);

    //internal static Func<T, int> GenerateHasher<T>()
    //{
    //  Type hashCodeType = typeof(HashCode);
    //  MethodInfo hashCodeMethod = hashCodeType.GetMethod("ToHashCode", BindingFlags.Public | BindingFlags.Instance);
    //  ConstructorInfo ctor = hashCodeType.GetConstructor(Array.Empty<Type>());
    //  var hashCode = Expression.Variable(hashCodeType, "hashCode");
    //  var returnTarget = Expression.Label();

    //  var ret = Expression.Return(returnTarget);
    //  var block = Expression.Block(
    //    hashCode,
    //    Expression.Assign(hashCode, Expression.New(ctor));

    //    Expression.Return(returnTarget, Expression.Call();
    //  Expression.Label(returnTarget)
    //    ) ;



    //  HashCode
    //  List<Expression> comparers = new List<Expression>();
    //  ParameterExpression obj = Expression.Parameter(typeof(T), "obj");

    //  foreach (PropertyInfo propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
    //  {
    //    if (propInfo.IsDefined(typeof(IgnoreAttribute)))
    //    {
    //      continue;
    //    }
    //    comparers.Add(GeneratePropertyExpression(obj, propInfo));
    //  }


    //  var andComparer = Expression.Lambda<Func<T, int>>(parameters, left).Compile();
    //  return andComparer;
    //}

    //}
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
    public int GetHashCode(T obj)
      => hasher(obj);

  }
}
