using System;
using System.Collections.Generic;
using System.Text;

namespace U2U.ValueObjectComparers
{
  [AttributeUsage(AttributeTargets.Property)]
  public class IgnoreAttribute : Attribute
  {
  }
}
