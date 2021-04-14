using System;
using Xunit.Sdk;

namespace Synnotech.Xunit
{
    /// <summary>
    /// Use this attribute to specify the execution order of xunit tests within a test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestOrderAttribute : Attribute, ITraitAttribute
    {
        /// <summary>
        /// The name of the trait that identifies test order values. The value is "TestOrder".
        /// </summary>
        public const string TraitName = "TestOrder";

        /// <summary>
        /// Initializes a new instance of <see cref="TestOrderAttribute"/> with the specified order value.
        /// </summary>
        /// <param name="order">
        /// The ordinal value that indicates the order of the test. It is recommended to start with 1
        /// and subsequently increase the order of each test.
        /// </param>
        public TestOrderAttribute(int order) => Order = order;

        /// <summary>
        /// Gets the ordinal value that indicates the order of the test.
        /// </summary>
        public int Order { get; }
    }
}