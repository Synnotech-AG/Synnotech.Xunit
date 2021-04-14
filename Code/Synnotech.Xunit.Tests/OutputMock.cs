using System;
using Xunit.Abstractions;

namespace Synnotech.Xunit.Tests
{
    public sealed class OutputMock : ITestOutputHelper
    {
        public string? CapturedMessage { get; private set; }

        public void WriteLine(string message) => CapturedMessage = message;

        public void WriteLine(string format, params object[] args) => throw new NotSupportedException();
    }
}