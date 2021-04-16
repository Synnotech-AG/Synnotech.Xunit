# Synnotech.Xunit

*Test extensions for xunit that make your life easier.*

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.Xunit/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.1.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Synnotech.Xunit/)

# How to Install

Synnotech.Xunit is compiled against [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and thus supports all major plattforms like .NET 5, .NET Core, .NET Framework 4.6.1 or newer, Mono, Xamarin, UWP, or Unity.

Synnotech.Xunit is available as a [NuGet package](https://www.nuget.org/packages/Synnotech.Xunit/) and can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Synnotech.Xunit" Version="1.1.0" />`
- **dotnet CLI**: `dotnet add package Synnotech.Xunit`
- **Visual Studio Package Manager Console**: `Install-Package Synnotech.Xunit`

# What does Synnotech.Xunit offer you?

## testsettings.json

Synnotech.Xunit comes with a class called `TestSettings` that allows you to customize tests via configuration files. It loads up to three optional configuration files ("testsettings.json", "testsettings.Development.json", and "testsettings.Build.json") and provides the `Configuration` property to access an `IConfiguration` instance that holds the loaded configuration. You can rely on all the goodness of Microsoft.Extensions.Configuration that you know from ASP.NET Core apps.

This is especially useful for integration tests where you target different databases on different test systems (e.g. individual dev machines and build server). Together with packages like [Xunit.SkippableFact](https://www.nuget.org/packages/Xunit.SkippableFact/), you can easily customize your integration tests.

A common use case might look like this:

In testsettings.json:
```jsonc
{
    // This file should be checked in your version control system and provide default settings.
    // Developers can individually set values in testsettings.Development.json.
    "database": {
        "areIntegrationTestsEnabled": false,
        "connectionString": "Server=(localdb)\\MsSqlLocalDb;Database=MyDatabase;Integrated Security=True"
    }
}
```

In testsettings.Development.json:
```jsonc
{
    // Developers can set individual values for their dev machine in this file
    // (we recommend you ignore this file in your version control system).
    "database": {
        "areIntegrationTestsEnabled": true,
        "connectionString": "Server=MyDevInstance;Database=MyDatabase;Integrated Security=True"
    }
}
```

Your test code:
```csharp
using Xunit;
using SynnotechTestSettings = Synnotech.Xunit.TestSettings;

namespace MyTestProject
{
    public static class TestSettings
    {
        public static void SkipDatabaseTestIfNecessary() =>
            Skip.IfNot(SynnotechTestSettings.Configuration.GetValue<bool>("database:areIntegrationTestsEnabled");
            
        public static string GetConnectionString() =>
            SynnotechTestSettings.Configuration["database:connectionString"];
    }
    
    public class DatabaseIntegrationTests
    {
        [SkippableFact] // This attribute is in Xunit.SkippableFact
        public async Task ConnectToDatabase()
        {
            TestSettings.SkipDatabaseTestIfNecessary();
            
            var connectionString = TestSettings.GetConnectionString();
            await using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
        }
    }
}
```

General recommendations for test settings:

- all integration tests that require a third-party system to be in place should be skippable and turned off by default.
- your version control system should ignore `testsettings.*.json` files. This way, each developer can create this file locally to provide custom settings according to his or her environment
- build servers should provide testsettings.Build.json to customize the settings for test runs in automated builds.

`TestSettings` will search for the three mentioned files in the working directory - please ensure to copy the corresponding json files to the output directory. You can achieve this by adding the following `ItemGroup` to your test csproj file:

```csproj
<Project Sdk="Microsoft.NET.Sdk">
    <!--Other groups omitted for sake of brevity-->
    <ItemGroup>
        <None Update="testsettings.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
        <None Update="testsettings.Development.json" Condition="Exists('testsettings.Development.json')">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
</Project>
```

## Run tests in a specific order

By default, xunit runs your tests in a non-deterministic / random order, even within a single test class. This is a good default behavior, because your tests should be independent from each other. However, especially when you write integration tests for a third-party system (e.g. a database or a web service) where you also need to manage its state, you might want to run your tests in a specific order. You can use Synnotech.Xunit's `TestOrderer` for these circumstances - it needs to be supplied to xunit's `TestCaseOrdererAttribute`:

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

Please keep in mind: if you apply the `TestOrderAttribute`, you need to apply it to all tests of a class - otherwise the `TestOrderer` will use a different the source code numbers to order your tests. And no matter if you use the `TestOrderAttribute` or not, you must decorate the corresponding class with `[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]`, otherwise the default ordering of xunit will be applied.

Furthermore, if you access the same third-party system in several test classes, you should generally consider running your test sequentially. Dependending on your data access layer, concurrent requests might lead to unexpected test outcome when two or more tests in different classes access the system concurrently. You can do that by placing the following lines of code into a new file in your xunit test project:

```csharp
using Xunit;

[assembly:CollectionBehavior(DisableTestParallelization = true)]
```

With this in place, all tests will be executed sequentially.

## Write strings and exceptions to ITestOutputHelper with ShouldBeWrittenTo

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
