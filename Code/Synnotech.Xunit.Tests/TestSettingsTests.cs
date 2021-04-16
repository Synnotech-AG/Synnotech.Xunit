using FluentAssertions;
using Xunit;

namespace Synnotech.Xunit.Tests
{
    /* The following tests use the hierarchy of testsettings.json, testsettings.Development.json, and testsettings.Build.json.
     * Check the files to see the three values and how they are overwritten. */
    public static class TestSettingsTests
    {
        [Fact]
        public static void ReadValue1() => TestSettings.Configuration["value1"].Should().Be("value 1 - in testsettings.json");

        [Fact]
        public static void ReadValue2() => TestSettings.Configuration["value2"].Should().Be("value 2 - overwritten in testsettings.Development.json");

        [Fact]
        public static void ReadValue3() => TestSettings.Configuration["value3"].Should().Be("value 3 - overwritten in testsettings.Build.json");
    }
}