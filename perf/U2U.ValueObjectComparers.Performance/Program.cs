using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchMarkSkeleton;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 0618

public class MainConfig : ManualConfig
{
  public MainConfig()
  {
    // You can use this to compare performance between different runtimes...
    //Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp20));
    Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp31));

    Add(MemoryDiagnoser.Default);
    Add(new MinimalColumnProvider());
    // Add(MemoryDiagnoser.Default.GetColumnProvider());
    //Set(new DefaultOrderProvider(SummaryOrderPolicy.SlowestToFastest));
    Add(MarkdownExporter.GitHub);
    Add(new ConsoleLogger());
  }

  private sealed class MinimalColumnProvider : IColumnProvider
  {
    public IEnumerable<IColumn> GetColumns(Summary summary)
    {
      yield return TargetMethodColumn.Method;
      //yield return new JobCharacteristicColumn(InfrastructureMode.ToolchainCharacteristic);
      yield return StatisticColumn.Min;
      yield return StatisticColumn.Mean;
      yield return StatisticColumn.Max;
    }
  }
}

public class Test
{
  public static void Main() => BenchmarkRunner.Run<Test>(new MainConfig());

  // ... benchmarks go here

  private const int fruityLoops = 2_000_000;
  private DateTime now = new DateTime(2019, 12, 24);

  private NestedValueObject myNested1 = new NestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private NestedValueObject myNested2 = new NestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private MSNestedValueObject msNested1 = new MSNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private MSNestedValueObject msNested2 = new MSNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };

  [Benchmark()]
  public void UsingMyValueObjectsThatAreEqual()
  {
    MyValueObject myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    MyValueObject myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (!shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectsThatAreEqualWithNesting()
  {
    MyValueObject myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested1 };
    MyValueObject myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested2 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (!shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectsThatAreNotEqual()
  {
    MyValueObject myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested1 };
    MyValueObject myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 44, Nested = myNested2 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreEqual()
  {
    MSValueObject msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    MSValueObject msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (!shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreEqualWithNesting()
  {
    MSValueObject msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested1 };
    MSValueObject msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested2 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (!shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreNotEqual()
  {
    MSValueObject msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested1 };
    MSValueObject msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 44, Nested = msNested2 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectStructsThatAreEqual()
  {
    MyValueObjectStruct obj1 = new MyValueObjectStruct { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    MyValueObjectStruct obj2 = new MyValueObjectStruct { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = obj1.Equals(obj2);
    if (!shouldBeTrue) throw new Exception();
    for (int i = 0; i < fruityLoops; i += 1)
    {
      ((IEquatable<MyValueObjectStruct>)obj1).Equals(obj2);
    }
  }
}