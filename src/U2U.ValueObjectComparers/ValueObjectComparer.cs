using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace U2U.ValueObjectComparers
{
  public sealed class ValueObjectComparer<T>
  {
    class NullComparer
    {
      private readonly Func<T, T, bool> right;

      public NullComparer(Func<T, T, bool> right)
        => this.right = right;

      public bool Compare(T x, T y) // ReferenceEquals(x,y) || ( !ReferenceEquals(x, null) && !ReferenceEquals(y, null) && right(x,y)
      {
        if (ReferenceEquals(x, y))
        {
          return true;
        }
        if (!ReferenceEquals(x, null) && !ReferenceEquals(y, null))
        {
          return right(x, y);
        }
        return false;
      }
    }

    public static ValueObjectComparer<T> Instance { get; } = new ValueObjectComparer<T>();

    private Func<T, T, bool> comparer = GenerateComparer();

    private static Expression GenerateEqualityExpression(MethodInfo equalMethod, ParameterExpression left, ParameterExpression right, PropertyInfo prop)
    {
      Expression equalCall = Expression.Call(Expression.Property(left, prop), equalMethod, Expression.Convert(Expression.Property(right, prop), typeof(object)));
      if (prop.PropertyType.IsValueType)
      {
        return equalCall;
      }
      else
      {
        Expression leftValue = Expression.Property(left, prop);
        Expression rightValue = Expression.Property(right, prop);
        Expression refEqual = Expression.ReferenceEqual(leftValue, rightValue);
        Expression nullConst = Expression.Constant(null);
        Expression leftIsNotNull = Expression.Not(Expression.ReferenceEqual(leftValue, nullConst));
        Expression rightIsNotNull = Expression.Not(Expression.ReferenceEqual(rightValue, nullConst));
        Expression neitherIsNull = Expression.AndAlso(leftIsNotNull, rightIsNotNull);

        Expression neitherIsNullAndIsEqual = Expression.AndAlso(neitherIsNull, equalCall);
        Expression either = Expression.OrElse(refEqual, neitherIsNullAndIsEqual);

        return either;
      }
    }

    private static Func<T, T, bool> GenerateComparer()
    {
      List<Expression> comparers = new List<Expression>();
      ParameterExpression left = Expression.Parameter(typeof(T), "left");
      ParameterExpression right = Expression.Parameter(typeof(T), "right");
      foreach (PropertyInfo propInfo in typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
      {
        MethodInfo equalMethod = propInfo.PropertyType.GetMethod(nameof(Equals), new Type[] { typeof(object) });
        comparers.Add(GenerateEqualityExpression(equalMethod, left, right, propInfo));
      }
      Expression ands = comparers.Aggregate((left, right) => Expression.AndAlso(left, right));
      Func<T, T, bool> andComparer = Expression.Lambda<Func<T, T, bool>>(ands, left, right).Compile();
      return new NullComparer(andComparer).Compare;
    }

    public bool Equals(in T left, in T right)
      => this.comparer(left, right);

    public bool Equals(T left, object right)
      => object.ReferenceEquals(left, right) || (right is object && right.GetType() == typeof(T) && Equals(left, (T)right));
  }
}
