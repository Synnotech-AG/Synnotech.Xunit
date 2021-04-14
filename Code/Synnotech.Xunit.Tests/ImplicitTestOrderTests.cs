using FluentAssertions;
using Xunit;

namespace Synnotech.Xunit.Tests
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
    public static class ImplicitTestOrderTests
    {
        private static int Value { get; set; } = 42;

        [Fact]
        public static void Test1()
        {
            Value.Should().Be(42);
            Value = 3;
        }

        [Fact]
        public static void Test2()
        {
            Value.Should().Be(3);
            Value = 5;
        }

        [Fact]
        public static void Test3()
        {
            Value.Should().Be(5);
            Value = 1988;
        }

        [Fact]
        public static void Test4()
        {
            Value.Should().Be(1988);
            Value = -40102;
        }

        [Fact]
        public static void Test5()
        {
            Value.Should().Be(-40102);
        }
    }
}