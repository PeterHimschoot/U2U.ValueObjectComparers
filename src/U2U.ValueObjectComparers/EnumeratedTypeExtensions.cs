using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2U.ValueObjectComparers
{
  public static class EnumeratedTypeExtensions
  {
    public static Type GetEnumeratedType(this Type type)
    {
      // provided by Array
      var elType = type.GetElementType();
      if (null != elType) return elType;

      // otherwise provided by collection
      var elTypes = type.GetGenericArguments();
      if (elTypes.Length > 0) return elTypes[0];

      // otherwise is not an 'enumerated' type
      return null;
    }
  }
}
