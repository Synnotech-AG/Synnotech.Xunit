using System;
using Light.GuardClauses;
using Xunit.Abstractions;

namespace Synnotech.Xunit
{
    /// <summary>
    /// Provides extensions for test scenarios.
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Writes the specified message to the test output helper.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> or <paramref name="output"/> is null.</exception>
        public static void ShouldBeWrittenTo(this string message, ITestOutputHelper output) =>
            output.MustNotBeNull(nameof(output)).WriteLine(message);

        /// <summary>
        /// Writes the specified exception to the test output helper.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> or <paramref name="output"/> is null.</exception>
        public static void ShouldBeWrittenTo(this Exception exception, ITestOutputHelper output)
        {
            exception.MustNotBeNull(nameof(exception));
            output.MustNotBeNull(nameof(output))
                  .WriteLine(exception.ToString());
        }
    }
}
