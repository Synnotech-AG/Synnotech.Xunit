using System;
using FluentAssertions;
using Xunit;

namespace Synnotech.Xunit.Tests
{
    public sealed class WriteExceptionToOutputTests
    {
        public OutputMock Output { get; } = new ();

        [Fact]
        public void WriteExceptionToOutput()
        {
            var exception = new Exception();

            exception.ShouldBeWrittenTo(Output);

            Output.CapturedMessage.Should().Be(exception.ToString());
        }

        [Fact]
        public static void OutputNull()
        {
            Action act = () => new Exception().ShouldBeWrittenTo(null!);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("output");
        }

        [Fact]
        public void ExceptionNull()
        {
            Action act = () => ((Exception) null!).ShouldBeWrittenTo(Output);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("exception");
        }
    }
}