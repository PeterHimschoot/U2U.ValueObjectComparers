using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using System;
using System.Collections.Generic;
using U2U.ValueObjectComparers;
#pragma warning disable 0618

public class MainConfig : ManualConfig
{
  public MainConfig()
  {
    // You can use this to compare performance between different runtimes...
    //Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp20));
    this.Add(Job.Default.With(Platform.X64).With(CsProjCoreToolchain.NetCoreApp31));

    this.Add(MemoryDiagnoser.Default);
    this.Add(new MinimalColumnProvider());
    // Add(MemoryDiagnoser.Default.GetColumnProvider());
    //Set(new DefaultOrderProvider(SummaryOrderPolicy.SlowestToFastest));
    this.Add(MarkdownExporter.GitHub);
    this.Add(new ConsoleLogger());
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
  private readonly DateTime now = new DateTime(2019, 12, 24);

  private readonly NestedValueObject myNested1 = new NestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private readonly NestedValueObject myNested2 = new NestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private readonly MSNestedValueObject msNested1 = new MSNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private readonly MSNestedValueObject msNested2 = new MSNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private readonly HCNestedValueObject hcNested1 = new HCNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };
  private readonly HCNestedValueObject hcNested2 = new HCNestedValueObject
  {
    Price = 100M,
    When = new DateTime(2019, 12, 24)
  };


  [Benchmark()]
  public void UsingHCValueObjectsThatAreEqual()
  {
    var obj1 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    var obj2 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = obj1.Equals(obj2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      obj1.Equals(obj2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectsThatAreEqual()
  {
    var myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    var myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreEqual()
  {
    var msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    var msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }


  [Benchmark()]
  public void UsingHCValueObjectsThatAreEqualWithNesting()
  {
    var myValueObject1 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = hcNested1 };
    var myValueObject2 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = hcNested2 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectsThatAreEqualWithNesting()
  {
    var myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested1 };
    var myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested2 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreEqualWithNesting()
  {
    var msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested1 };
    var msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested2 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }

  [Benchmark()]
  public void UsingSameInstanceOfHCValueObject()
  {
    var myValueObject1 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = hcNested1 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject1);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject1);
    }
  }

  [Benchmark()]
  public void UsingSameInstanceOfMyValueObject()
  {
    var myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested1 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject1);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject1);
    }
  }

  [Benchmark()]
  public void UsingSameInstanceOfMSValueObject()
  {
    var msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject1);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject1);
    }
  }

  [Benchmark()]
  public void UsingHCValueObjectsThatAreNotEqual()
  {
    var obj1 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = hcNested1 };
    var obj2 = new HCValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 44, Nested = hcNested2 };
    bool shouldBeTrue = obj1.Equals(obj2);
    if (shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      obj1.Equals(obj2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectsThatAreNotEqual()
  {
    var myValueObject1 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = myNested1 };
    var myValueObject2 = new MyValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 44, Nested = myNested2 };

    bool shouldBeTrue = myValueObject1.Equals(myValueObject2);
    if (shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      myValueObject1.Equals(myValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMSValueObjectsThatAreNotEqual()
  {
    var msValueObject1 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43, Nested = msNested1 };
    var msValueObject2 = new MSValueObject { FirstName = "Jefke", LastName = "Vandersmossen", Age = 44, Nested = msNested2 };
    bool shouldBeTrue = msValueObject1.Equals(msValueObject2);
    if (shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      msValueObject1.Equals(msValueObject2);
    }
  }

  [Benchmark()]
  public void UsingMyValueObjectStructsThatAreEqual()
  {
    var obj1 = new MyValueObjectStruct { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    var obj2 = new MyValueObjectStruct { FirstName = "Jefke", LastName = "Vandersmossen", Age = 43 };
    bool shouldBeTrue = obj1.Equals(obj2);
    if (!shouldBeTrue)
    {
      throw new Exception();
    }

    for (int i = 0; i < fruityLoops; i += 1)
    {
      ((IEquatable<MyValueObjectStruct>)obj1).Equals(obj2);
    }
  }
}