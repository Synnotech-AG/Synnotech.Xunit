# Synnotech.Xunit

*Test extensions for xunit that make your like easier.*

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.Xunit/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.0.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Synnotech.Xunit/)

## How to Install

Synnotech.Xunit is compiled against [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and thus supports all major plattforms like .NET 5, .NET Core, .NET Framework 4.6.1 or newer, Mono, Xamarin, UWP, or Unity.

Synnotech.Xunit is available as a [NuGet package](https://www.nuget.org/packages/Synnotech.Xunit/) and can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Synnotech.Xunit" Version="1.0.0" />`
- **dotnet CLI**: `dotnet add package Synnotech.Xunit`
- **Visual Studio Package Manager Console**: `Install-Package Synnotech.Xunit`

## What does Synnotech.Xunit offer you?

### Run tests in a specific order

By default, xunit runs your tests in a non-deterministic / random order, even within a single test class. This is a good default behavior, because your tests should be independent from each other. However, especially when you write integration tests to a third-party system (e.g. a database or a web service) where you also need to manage the its state, you might want to structure your tests in a way that they are run in a specific order. You can use Synnotech.Xunit's `TestOrderer` for these circumstances - it needs to be supplied to xunit's `TestCaseOrdererAttribute`:

```csharp
using Synnotech.Xunit;
using Xunit;

namespace TestProject
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)] // This line enables TestOrderer on this test class
    public class TestClass
    {
        private static int Value { get; set; } = 1;
    
        [Fact]
        public void Test1()
        {
            Assert.Equal(1, Value);
            Value = 33;
        }
        
        [Fact]
        public void Test2()
        {
            Assert.Equal(33, Value);
            Value = 7;
        }
        
        [Fact]
        public void Test3()
        {
            Assert.Equal(7, Value);
        }
    }
}
```

By simply applying the `TestOrderer` to the test class, the containing tests will be ordered according to their source code line number. Thus if you simply want to run the tests in the same order as they occur in your test class, then you are done. However, if you want to specify the order of the tests on your own, you can do that with the `TestOrderAttribute`:


```csharp
using Synnotech.Xunit;
using Xunit;

namespace TestProject
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)] // This line enables TestOrderer on this test class
    public class TestClass
    {
        private static int Value { get; set; } = 1;
    
        [Fact]
        [TestOrder(3)]
        public void Test1()
        {
            Assert.Equal(7, Value);
        }
        
        [Fact]
        [TestOrder(1)]
        public void Test2()
        {
            Assert.Equal(1, Value);
            Value = 33;
        }
        
        [Fact]
        [TestOrder(2)]
        public void Test3()
        {
            Assert.Equal(33, Value);
            Value = 7;
        }
    }
}
```

Please keep in mind: no matter if you use the `TestOrderAttribute` or not, you must decorate the corresponding class with `[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]`, otherwise the default ordering of xunit will be applied.

Furthermore, if you access the same third-party system in several test classes, you should generally consider running your test sequentially. Dependending on your data access layer, concurrent requests might lead to unexpected test outcome when two or more tests in different classes access the system concurrently. You can do that by placing the following lines of code into a new file in your xunit test project:

```csharp
using Xunit;

[assembly:CollectionBehavior(DisableTestParallelization = true)]
```

With this in place, all tests are run one after another.

### Write strings and exceptions to ITestOutputHelper with ShouldBeWrittenTo

Synnotech.Xunit contains two extension methods that allow you to easily write `string` or `Exception` instances to an `ITestOutputHelper`. This is especially useful in combination with [FluentAssertions](https://fluentassertions.com/)

```csharp
public sealed class OrderProcessorTests
{
    public TestClass(ITestOutputHelper output) => Output = output;
    
    private ITestOutputHelper Output { get; }
    
    [Fact]
    public void CancelledOrdersShouldNotBeProcessed()
    {
        Action act => () => OrderProcessor.ProcessOrders(new Order { Id = 1, State = State.Cancelled } );
        
        act.Should().Throw<ProcessingException>()
           .Which.ShouldBeWrittenTo(Output); // as an alternative: .And.Message.ShouldBeWrittenTo(Output)
    }
}
```
