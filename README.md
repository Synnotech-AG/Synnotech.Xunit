# Synnotech.Xunit

*Test extensions for xunit that make your life easier.*

[![Synnotech Logo](synnotech-large-logo.png)](https://www.synnotech.de/)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Synnotech-AG/Synnotech.Xunit/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.2.1-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Synnotech.Xunit/)

# How to Install

Synnotech.Xunit is compiled against [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) and thus supports all major plattforms like .NET 6, .NET Core, .NET Framework 4.6.1 or newer, Mono, Xamarin, UWP, or Unity.

Synnotech.Xunit is available as a [NuGet package](https://www.nuget.org/packages/Synnotech.Xunit/) and can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Synnotech.Xunit" Version="1.2.1" />`
- **dotnet CLI**: `dotnet add package Synnotech.Xunit`
- **Visual Studio Package Manager Console**: `Install-Package Synnotech.Xunit`

# What does Synnotech.Xunit offer you?

## testsettings.json

Synnotech.Xunit comes with a class called `TestSettings` that allows you to customize tests via configuration files. It loads up to three optional configuration files ("testsettings.json", "testsettings.Development.json", and "testsettings.Build.json") and provides the `Configuration` property to access an `IConfiguration` instance that holds the loaded configuration. You can rely on all the goodness of Microsoft.Extensions.Configuration that you know from ASP.NET Core apps.

This is especially useful for integration tests where you target different databases on different test systems (e.g. individual dev machines vs. build server). Together with packages like [Xunit.SkippableFact](https://www.nuget.org/packages/Xunit.SkippableFact/), you can easily customize your integration tests.

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

namespace MyTestProject;

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
        
        // Test something using the sqlConnection
    }
}
```

General recommendations for test settings:

- all integration tests that require a third-party system to be in place should be skippable and turned off by default.
- your version control system should ignore `testsettings.*.json` files. This way, each developer can create this file locally to provide custom settings according to his or her environment
- build servers should provide testsettings.Build.json to customize the settings for test runs in automated builds.

The three testsettings files will be automatically copied to the output directory when you place them next to the csproj file of your test project. When loading these files, Synnotech.Xunit will determine if it should run in Build Server mode. In Build Server mode, the `IConfiguration` instance will only contain testsettings.json, testsettings.Build.json, and environment variables. In non Build Server mode, testsettings.Build.json is replaced with testsettings.Development.json. Synnotech.Xunit determines this by trying to load all three files, determining if Build Server mode is active, and then loading the configuration again with only the files you need (the reason for this complex approach is that there is no dedicated Composition Root in xunit that we could hook into).

You usually want to use an environment variable that is defined on your build server, but not on your dev machines (e.g. GitLab defines an environment variable called "CI" which is set to "True") to determine whether Build Server mode is active. You can customize this behavior by placing a dedicated section called "testConfiguration" in one of your JSON files (typically testsettings.json). This section is structured like this:

```jsonc
{
    "testConfiguration": {

        // The name of the environment variable that determines whether Build Server mode is active.
        // The value of this environment variable must be either "True" or "1" so that 
        // Synnotech.Xunit actives this mode.
        "isInBuildServerModeEnvironmentVariableName": "CI", 

         // The value indicating whether environment variables should be loaded, too
        "loadEnvironmentVariables": true,

        // If you set this value, only the environment variables with this prefix will be loaded.
        // The prefix will be stripped from environment variable name when it is added to
        // the IConfiguration instance (default behavior of Microsoft.Extensions.Configuration).
        "environmentVariablesPrefix": "SynnotechTest_", 

        // The value indicating whether the testsettings.Development.json file should also
        // be loaded in Build Server mode. Usually, you only want to set this value to true
        // when you need to replicate a certain behavior of the build server on a local
        // dev machine.
        "loadDevelopmentSettingsFileInBuildServerMode": false
    }
}
```

If you want even more control, you can use the `TestSettings.LoadConfiguration` method instead of accessing the default `TestSettings.Configuration` instance. Check its XML comments for detailed instructions.

## Run tests in a specific order

By default, xunit runs your tests in a non-deterministic / random order, even within a single test class. This is a good default behavior, because your tests should be independent from each other. However, especially when you write integration tests for a third-party system (e.g. a database or a web service) where you also need to manage its state, you might want to run your tests in a specific order. You can use Synnotech.Xunit's `TestOrderer` for these circumstances - it needs to be supplied to xunit's `TestCaseOrdererAttribute`:

```csharp
using Synnotech.Xunit;
using Xunit;

namespace TestProject
{
    // The following attribute enables TestOrderer on this test class
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)] 
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
    // The following attribute enables TestOrderer on this test class
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
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

Please keep in mind: when you apply the `TestOrderAttribute`, you need to apply it to all tests of a class - otherwise the `TestOrderer` will use the line numbers to order your tests. And no matter whether you use the `TestOrderAttribute` or not, you must decorate the corresponding class with `[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]`, otherwise the default ordering of xunit will be applied.

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
