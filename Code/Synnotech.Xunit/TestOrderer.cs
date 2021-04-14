using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Synnotech.Xunit
{
    /// <summary>
    /// Represents a test case orderer that uses the "TestOrder" trait to order test cases.
    /// </summary>
    public sealed class TestOrderer : ITestCaseOrderer
    {
        /// <summary>
        /// Gets the type name of this test case orderer.
        /// </summary>
        public const string TypeName = AssemblyInfos.AssemblyName + "." + nameof(TestOrderer);

        /// <summary>
        /// Gets the assembly name where this test case orderer is located in.
        /// </summary>
        public const string AssemblyName = AssemblyInfos.AssemblyName;

        /// <summary>
        /// Orders the test cases according to the value of trait "TestOrder". If the corresponding value
        /// cannot be cast to an int, int.MaxValue will be assigned to the corresponding test case.
        /// </summary>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
            testCases.MustNotBeNull(nameof(testCases))
                     .OrderBy(testCase =>
                      {
                          if (testCase.Traits.TryGetValue(TestOrderAttribute.TraitName, out var values) &&
                              values.Count > 0 &&
                              int.TryParse(values[0], out var testOrder))
                          {
                              return testOrder;
                          }

                          return int.MaxValue;
                      });
    }
}