using FluentAssertions;
using Xunit;

namespace Synnotech.Xunit.Tests
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
    public static class ExplicitTestOrderTests
    {
        private static int Value { get; set; } = 1;

        [Fact]
        [TestOrder(4)]
        public static void Test4()
        {
            Value.Should().Be(4);
            Value = 5;
        }

        [Fact]
        [TestOrder(1)]
        [Trait("Category", "Some other trait")] // This attribute is here to ensure that any other trait still works
        public static void Test1()
        {
            Value.Should().Be(1);
            Value = 2;
        }

        [Fact]
        [TestOrder(3)]
        public static void Test3()
        {
            Value.Should().Be(3);
            Value = 4;
        }

        [Fact]
        [TestOrder(2)]
        public static void Test2()
        {
            Value.Should().Be(2);
            Value = 3;
        }
    }
}