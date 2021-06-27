using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationShared
{
    public class ConfirmMatchingTests
    {
        private ITestOutputHelper _output;

        public ConfirmMatchingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Should find matching tests for all shared features between implementations")]
        public void TestClassesMatchingTests()
        {
            var sourceClass1 = typeof(ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading.BulkLoaderForBatchSizeTests)
                .Assembly;
            var sourceClass2 = typeof(ivaldez.Sql.IntegrationSqlServerTests.BulkLoading.BulkLoaderForBatchSizeTests)
                .Assembly;

            var allTestMethods1 = sourceClass1.GetTypes().Where(x => x.Name.ToLower().EndsWith("tests")).Select(x => x.Name).ToArray();
            var allTestMethods2 = sourceClass2.GetTypes().Where(x => x.Name.ToLower().EndsWith("tests")).Select(x => x.Name).ToArray();

            var testsMIssingIn1 = allTestMethods1.Except(allTestMethods2).ToArray();
            var testsMIssingIn2 = allTestMethods2.Except(allTestMethods1).ToArray();

            _output.WriteLine($"Test classes missing in: {sourceClass1}");
            foreach (var missing in testsMIssingIn1)
            {
                _output.WriteLine($"Test classes missing: {missing}");
            }

            _output.WriteLine($"Test classes missing in: {sourceClass2}");
            foreach (var missing in testsMIssingIn2)
            {
                _output.WriteLine($"Test classes missing: {missing}");
            }

            testsMIssingIn1.Length.Should().Be(0);
            testsMIssingIn2.Length.Should().Be(0);
        }

        [Fact(DisplayName = "Should find matching test methods for all shared features between implementations")]
        public void TestMethodMatchingTests()
        {
            var sourceClass1 = typeof(ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading.BulkLoaderForBatchSizeTests)
                .Assembly;
            var sourceClass2 = typeof(ivaldez.Sql.IntegrationSqlServerTests.BulkLoading.BulkLoaderForBatchSizeTests)
                .Assembly;

            var allTestClasses1 = sourceClass1.GetTypes().Where(x => x.IsClass && x.Name.ToLower().EndsWith("tests")).ToArray();
            var allTestClasses2 = sourceClass2.GetTypes().Where(x => x.IsClass && x.Name.ToLower().EndsWith("tests")).ToArray();

            var errorCount = 0;
            foreach (var classDefinition in allTestClasses1)
            {
                var matchingType = allTestClasses2.First(x => x.Name == classDefinition.Name);

                var allTestMethods1 = classDefinition.GetMethods().Select(x => x.Name).ToArray();
                var allTestMethods2 = matchingType.GetMethods().Select(x => x.Name).ToArray();

                var testsMIssingIn1 = allTestMethods1.Except(allTestMethods2).ToArray();
                var testsMIssingIn2 = allTestMethods2.Except(allTestMethods1).ToArray();

                _output.WriteLine($"Test class: {classDefinition}");
                foreach (var missing in testsMIssingIn1)
                {
                    errorCount++;
                    _output.WriteLine($"***missing method: {missing}");
                }

                _output.WriteLine($"Test class: {matchingType}");
                foreach (var missing in testsMIssingIn2)
                {
                    errorCount++;
                    _output.WriteLine($"***missing method: {missing}");
                }

                _output.WriteLine($"");
                _output.WriteLine($"-----------------------");
            }

            errorCount.Should().Be(0);
        }
    }
}
