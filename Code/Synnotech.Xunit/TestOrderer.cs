using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Synnotech.Xunit
{
    /// <summary>
    /// Represents a test case orderer that orders test cases according to their code line number by default.
    /// Additionally, tests can be explicitly ordered by using the <see cref="TestOrderAttribute"/>.
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
        /// Tries to order the test cases according to the "TestOrder" trait. If that is not possible,
        /// the tests are ordered by their code line number.
        /// </summary>
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var list = testCases.MustNotBeNull(nameof(testCases)).ToList();
            if (CheckIfTestOrderTraitIsPresent(list))
                return OrderByTrait(list);
            if (CheckIfSourceInformationIsPresent(list))
                return OrderByLineNumber(list);
            return list;
        }

        private static bool CheckIfTestOrderTraitIsPresent<TTestCase>(List<TTestCase> list) where TTestCase : ITestCase
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Traits.ContainsKey(TestOrderAttribute.TraitName))
                    return true;
            }

            return false;
        }

        private static IEnumerable<TTestCase> OrderByTrait<TTestCase>(List<TTestCase> list) where TTestCase : ITestCase =>
            list.OrderBy(testCase =>
            {
                if (testCase.Traits.TryGetValue(TestOrderAttribute.TraitName, out var values) &&
                    values.Count > 0 &&
                    int.TryParse(values[0], out var testOrder))
                {
                    return testOrder;
                }

                return int.MaxValue;
            });

        private static bool CheckIfSourceInformationIsPresent<TTestCase>(List<TTestCase> list) where TTestCase : ITestCase
        {
            for (var i = 0; i < list.Count; i++)
            {
                var testCase = list[i];
                if (testCase.SourceInformation == null || testCase.SourceInformation.LineNumber == null)
                    return false;
            }
            return true;
        }

        private static IEnumerable<TTestCase> OrderByLineNumber<TTestCase>(List<TTestCase> list) where TTestCase : ITestCase =>
            list.OrderBy(testCase => testCase.SourceInformation!.LineNumber!.Value);
    }
}