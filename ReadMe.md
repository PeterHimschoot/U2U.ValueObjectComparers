# Implementing ValueObject's Equality - Efficiently

![Equality](https://blogs.u2u.be/peter/peter/image.axd?picture=/Equality.jpg)

This blog discusses how to implement a Value Object's Equals method efficiently.

## What are Value Objects?

In **Domain Driven Design** objects are divided  into two groups: **Entities** and **Value Objects**. 

Entities are objects that have an identity and life cycle.

Value Objects are objects that don't have any real identity and are mainly used to describe aspects of an entity, such as your name which is of type string.
`String` is definitely a value object, because with value objects you don't care which instance you are holding.
For example, when writing one a whiteboard you want to use a blue marker. If you have many markers which are blue, do you care which one you are holding? 
If so, then that marker is an entity, if not it is a value object. Entities are equal when they have the same identity, value objects are equal when all properties that define one are equal.

## Implementing Equality for Value Objects

To implement equality for a value object we need to compare each of its properties for equality (You could say that a value object's identity is defined by all of its properties). This is not hard, but it is repetitive work. 
Each time you add a new property you have to update the `Equals` method to use that property too.

### The Microsoft Approach

There are implementations for a `ValueObject` base class, which takes care of most of the work, for example, the one from [Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects).

Here you need to override the `GetAtomicValues` method.

In case of `Address` (copied from Docs):

``` csharp
protected override IEnumerable<object> GetAtomicValues()
{
  // Using a yield return statement to return each element one at a time
  yield return Street;
  yield return City;
  yield return State;
  yield return Country;
  yield return ZipCode;
}
```

Pretty simple, but I have two problems with this. 

First of all, you need to inherit from the `ValueObject` base class. 
This excludes the use of Value Types as a Value Object. 
Value types (`struct`) are ideal Value Objects because they get embedded in the entities, just like built-in value objects `int` and others.

The second objection is that you should not forget to add an extra property to this method each time you add a property to the type...

### Using Reflection

So what is the solution? Of course, you could use reflection to implement `Equals` like [here](https://github.com/jhewlett/ValueObject).
In this case, reflection automatically discovers all the properties and compares all of them, returning true is all properties are equal.

The problem with reflection is that it is slow, so you should limit reflection to "just once".

![/slow.png](https://blogs.u2u.be/peter/peter/image.axd?picture=/slow.png)

### "Just Once" Reflection

There is a third approach where you use reflection to figure out what to do and generate the code so the second time things go really fast.
That is the approach I took to build `ValueObjectComparer<T>`.

Here is an example of what a Value Object looks like. 
The `Equals` method simply delegates to the `ValueObjectComparer<SomeObject>`.
Same for the `IEquatable<SomeObject>` interface implementation.

```csharp
public class SomeObject : IEquatable<SomeObject>
{
  public string Name { get; set; }
  public int Age { get; set; }

  public override bool Equals(object obj)
    => ValueObjectComparer<SomeObject>.Instance.Equals(this, obj);

  public bool Equals([AllowNull] SomeObject other) 
    => ValueObjectComparer<SomeObject>.Instance.Equals(this, other);
}
```

## Performance

Let's see how the performance compares between the 'Equals' as prescribed by Microsoft or the "Just Once" reflection implementation.
For this I have used the excellent [BenchmarkDotNet](https://benchmarkdotnet.org/) library, and here are the results:

```
|                                     Method |       Mean |        Min |        Max |
|------------------------------------------- |-----------:|-----------:|-----------:|
|            UsingHCValueObjectsThatAreEqual |  22.704 ms |  21.129 ms |  24.983 ms |
|            UsingMyValueObjectsThatAreEqual |  33.040 ms |  32.018 ms |  33.464 ms |
|            UsingMSValueObjectsThatAreEqual | 284.574 ms | 281.209 ms | 296.570 ms |
| UsingHCValueObjectsThatAreEqualWithNesting |  46.565 ms |  44.717 ms |  49.586 ms |
| UsingMyValueObjectsThatAreEqualWithNesting |  88.113 ms |  87.233 ms |  88.731 ms |
| UsingMSValueObjectsThatAreEqualWithNesting | 619.700 ms | 608.544 ms | 636.450 ms |
|           UsingSameInstanceOfHCValueObject |   3.818 ms |   3.642 ms |   4.292 ms |
|           UsingSameInstanceOfMyValueObject |   5.428 ms |   5.215 ms |   6.025 ms |
|           UsingSameInstanceOfMSValueObject |   3.742 ms |   3.708 ms |   3.784 ms |
|         UsingHCValueObjectsThatAreNotEqual |  21.543 ms |  20.937 ms |  22.911 ms |
|         UsingMyValueObjectsThatAreNotEqual |  30.257 ms |  29.569 ms |  31.927 ms |
|         UsingMSValueObjectsThatAreNotEqual | 244.008 ms | 233.140 ms | 260.072 ms |
|      UsingMyValueObjectStructsThatAreEqual |  90.359 ms |  87.499 ms |  93.583 ms |
```

Using `ValueObjectComparer<T>` results in about a 10x faster implementation then the Microsoft implementation, but is one-third slower then the hardcoded version.

![Speedy](https://blogs.u2u.be/peter/peter/image.axd?picture=/speedy.gif)

The `UsingHCValueObjectsThatAreEqual` method uses a hardcoded implementation for `Equals`:

```csharp
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
```

The `UsingMyValueObjectsThatAreEqual` method measures performance of using the `ValueObjectComparer<T>` with a `class`:

```csharp
public class MyValueObject
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public int Age { get; set; }
  public NestedValueObject Nested { get; set; }
  public override bool Equals(object obj)
    => ValueObjectComparer<MyValueObject>.Instance.Equals(this, obj);
}
```

The similar `UsingMSValueObjectsThatAreEqual` method uses the Microsoft implementation:

```csharp
public class MSValueObject : ValueObject
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public int Age { get; set; }
  public MSNestedValueObject Nested { get; set; }

  protected override IEnumerable<object> GetAtomicValues()
  {
    yield return this.FirstName;
    yield return this.LastName;
    yield return this.Age;
    yield return this.Nested;
  }
}
```

In this test we use two instances of the same (equal) value object and the `Nested` property is `null`.
We call `Equals` 2_000_000 times after a check that these are equal:

```csharp
private const int fruityLoops = 2_000_000;
```

```csharp
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
```

## Value objects are immutable

Value Objects are immutable, and in this case we can use the **Flyweight** pattern.

In this case the first `object.ReferenceEquals` will hit most of the time, with this performance as a result:

```
|                           Method |       Mean |        Min |        Max |
|--------------------------------- |-----------:|-----------:|-----------:|
| UsingSameInstanceOfHCValueObject |   3.589 ms |   3.440 ms |   4.016 ms |
| UsingSameInstanceOfMyValueObject |   5.449 ms |   5.394 ms |   5.555 ms |
| UsingSameInstanceOfMSValueObject | 311.271 ms | 294.446 ms | 340.119 ms |
```

As you can see, the Docs implementation is a lot slower, mainly because they don't use the `object.ReferenceEquals`.
Adding this check to `Equals` results in comparable performance as the rest.

> When possible, use the Flyweight pattern for your value objects. Using this pattern for much used values can give a significant performance benefit!

## Value Objects can be .NET Value Types

The last performance test `UsingMyValueObjectStructsThatAreEqual` uses a `struct` instead of a `class`:

```csharp
public struct MyValueObjectStruct : IEquatable<MyValueObjectStruct>
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public int Age { get; set; }
  public override bool Equals(object obj)
    => ValueObjectComparer<MyValueObjectStruct>.Instance.Equals(this, obj);
  public bool Equals([AllowNull] MyValueObjectStruct other)
    => ValueObjectComparer<MyValueObjectStruct>.Instance.Equals(this, other);
}
```

With struct we need to be extra careful that they don't get boxed, and that is why implementing `IQuatable<T>` is essential.

 











# Interesting links

[Prefer ValueTask to Task, always](https://blog.marcgravell.com/2019/08/prefer-valuetask-to-task-always-and.html)

[Register service with multiple interfaces](https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/)

[Fluent Assertions](https://fluentassertions.com/)

[Shadow Properties](https://docs.microsoft.com/en-us/ef/core/modeling/shadow-properties)

[Mapping Well Designed Domain Models with EF](https://www.youtube.com/watch?v=9Vp2iXlhK-s)

[DDD Friendlier EF Core](https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/september/data-points-ddd-friendlier-ef-core-2-0)
[DDD Friendlier EF Core 2](https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/october/data-points-ddd-friendlier-ef-core-2-0-part-2)
[EF Core Query Types](https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/july/data-points-ef-core-2-1-query-types)
[DDD Tips and Tricks](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/august/data-points-coding-for-domain-driven-design-tips-for-data-focused-devs)
[Bounded Contexts](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/january/data-points-shrink-ef-models-with-ddd-bounded-contexts)

[Fake Data Generator Bogus](https://github.com/bchavez/Bogus)

## Entities

[Entity Design](https://www.thereformedprogrammer.net/creating-domain-driven-design-entity-classes-with-entity-framework-core/)
[Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)

## Value Objects

Default implementation for [GetHashCode](https://www.tabsoverspaces.com/233725-easier-gethashcode-implementation-in-net-core-2-1)

Implementing [Value Objects](https://dotnetcultist.com/ddd-value-objects-with-entity-framework-core/)

Entity Type Configuration - [Seperatly](https://stackoverflow.com/questions/26957519/ef-core-mapping-entitytypeconfiguration)

## Blogs

(.NET Cultist)[https://dotnetcultist.com/category/domain-driven-design/]
