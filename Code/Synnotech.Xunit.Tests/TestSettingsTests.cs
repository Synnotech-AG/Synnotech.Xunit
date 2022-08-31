using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Synnotech.Xunit.Tests;

public sealed class TestSettingsTests
{
    public TestSettingsTests()
    {
        DeleteFileIfNecessary("testsettings.json");
        DeleteFileIfNecessary("testsettings.Build.json");
        DeleteFileIfNecessary("testsettings.Development.json");
    }

    [Fact]
    public void LoadNoSettings() =>
        TestSettings.LoadConfiguration().GetChildren().Should().BeEmpty();

    [Fact]
    public void LoadOnlyTestSettings()
    {
        File.WriteAllText("testsettings.json", "{ \"someValue\": \"Foo\" }");

        var configuration = TestSettings.LoadConfiguration();
        var someValue = configuration["someValue"];

        someValue.Should().Be("Foo");
    }

    [Fact]
    public void OverwriteValueInDevelopmentSettings()
    {
        File.WriteAllText("testsettings.json", "{ \"aValue\": \"Foo\" }");
        File.WriteAllText("testsettings.Development.json", "{ \"aValue\": \"Bar\" }");

        var configuration = TestSettings.LoadConfiguration();
        var aValue = configuration["aValue"];

        aValue.Should().Be("Bar");
    }

    [Fact]
    public void IgnoreBuildSettingsWhenBuildServerModeIsOff()
    {
        File.WriteAllText("testsettings.json", "{ \"theValue\": \"Foo\" }");
        File.WriteAllText("testsettings.Development.json", "{ \"theValue\": \"Bar\" }");
        File.WriteAllText("testsettings.Build.json", "{ \"theValue\": \"Baz\" }");

        var configuration = TestSettings.LoadConfiguration();
        var theValue = configuration["theValue"];

        theValue.Should().Be("Bar");
        configuration.Providers.Should().HaveCount(2, "testSettings.Build.json was not loaded");
    }

    [Fact]
    public void IgnoreDevelopmentSettingsWhenBuildServerModeIsOn()
    {
        File.WriteAllText("testsettings.json", "{ \"theValue\": \"Foo\" }");
        File.WriteAllText("testsettings.Development.json", "{ \"theValue\": \"Bar\" }");
        File.WriteAllText("testsettings.Build.json", "{ \"theValue\": \"Baz\" }");

        var configuration = TestSettings.LoadConfiguration(isInBuildServerMode: true);
        var theValue = configuration["theValue"];

        theValue.Should().Be("Baz");
        configuration.Providers.Should().HaveCount(2, "testsettings.Development.json was not loaded");
    }

    [Fact]
    public void IncludeDevelopmentSettingsInBuildServerMode()
    {
        File.WriteAllText("testsettings.json", "{ \"theValue\": \"Foo\" }");
        File.WriteAllText("testsettings.Development.json", "{ \"testConfiguration\": { \"loadDevelopmentSettingsFileInBuildServerMode\": true }, \"theValue\": \"Bar\" }");
        File.WriteAllText("testsettings.Build.json", "{ \"theValue\": \"Baz\" }");

        var configuration = TestSettings.LoadConfiguration(isInBuildServerMode: true);
        var theValue = configuration["theValue"];

        theValue.Should().Be("Bar");
        configuration.Providers.Should().HaveCount(3, "all JSON file were included because of loadDevelopmentSettingsFileInBuildServerMode");
    }

    [Fact]
    public void LoadEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("SynnotechXunit_TestVariable", "Foo", EnvironmentVariableTarget.Process);

        File.WriteAllText("testsettings.json", "{ \"testConfiguration\": { \"loadEnvironmentVariables\": true, \"environmentVariablesPrefix\": \"SynnotechXunit_\" }, \"theValue\": \"Foo\" }");

        var configuration = TestSettings.LoadConfiguration();
        var environmentVariableValue = configuration["TestVariable"];

        environmentVariableValue.Should().Be("Foo");
    }

    private static void DeleteFileIfNecessary(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}