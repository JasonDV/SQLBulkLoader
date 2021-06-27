using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SharedTestFramework;
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

        private class ConfirmMatchingTestHelper
        {
            public ConfirmMatchingTestHelper()
            {
                PostgreAssembly = typeof(ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading.BulkLoaderForBatchSizeTests)
                    .Assembly;
                SqlServerAssembly = typeof(ivaldez.Sql.IntegrationSqlServerTests.BulkLoading.BulkLoaderForBatchSizeTests)
                    .Assembly;

                var postgreAssemblyName = PostgreAssembly.GetName().Name;
                var sqlServerAssemblyName = SqlServerAssembly.GetName().Name;

                PostgreClasses = PostgreAssembly.GetTypes().Where(x => x.Name.ToLower().EndsWith("tests")).ToArray();
                SqlServerClasses = SqlServerAssembly.GetTypes().Where(x => x.Name.ToLower().EndsWith("tests")).ToArray();

                PostgreClassInfo = PostgreClasses.Select(x => new ClassInfo
                    {
                        Class = x,
                        Assembly = x.Assembly,
                        ReferenceName = x.FullName.Replace(postgreAssemblyName, ""),
                        Ignore = ShouldIgnore(x)
                    })
                    .Where(x => x.Ignore == false).ToArray();
                SqlServerClassInfo = SqlServerClasses.Select(x => new ClassInfo
                    {
                        Class = x,
                        Assembly = x.Assembly,
                        ReferenceName = x.FullName.Replace(sqlServerAssemblyName, ""),
                        Ignore = ShouldIgnore(x)
                    })
                    .Where(x => x.Ignore == false).ToArray();
            }

            private bool ShouldIgnore(Type type)
            {
                Attribute[] attrs = System.Attribute.GetCustomAttributes(type);

                return attrs.Any(x => x.GetType().FullName == typeof(ImplementationSpecificTestAttribute).FullName);
            }

            public class ClassInfo
            {
                public string ReferenceName { get; set; }
                public Assembly Assembly { get; set; }
                public bool Ignore { get; set; }
                public Type Class { get; set; }
            }

            public Type[] PostgreClasses { get; set; }
            
            public Type[] SqlServerClasses { get; set; }

            public ClassInfo[] SqlServerClassInfo { get; set; }

            public ClassInfo[] PostgreClassInfo { get; set; }

            public Assembly SqlServerAssembly { get; set; }

            public Assembly PostgreAssembly { get; set; }
        }

        [Fact(DisplayName = "Should find matching tests for all shared features between implementations")]
        public void TestClassesMatchingTests()
        {
            var helper = new ConfirmMatchingTestHelper();

            var postgreClasses = helper.PostgreClassInfo.Select(x => x.ReferenceName).ToArray();
            var sqlServerClasses = helper.SqlServerClassInfo.Select(x => x.ReferenceName).ToArray();

            var postgreMissingClasses = postgreClasses.Except(sqlServerClasses).ToArray();
            var sqlServerMissingClasses = sqlServerClasses.Except(postgreClasses).ToArray();

            _output.WriteLine($"Test classes missing in: {helper.PostgreAssembly}");
            foreach (var missing in postgreMissingClasses)
            {
                _output.WriteLine($"Test classes missing: {missing}");
            }

            _output.WriteLine($"Test classes missing in: {helper.SqlServerAssembly}");
            foreach (var missing in sqlServerMissingClasses)
            {
                _output.WriteLine($"Test classes missing: {missing}");
            }

            postgreMissingClasses.Length.Should().Be(0);
            sqlServerMissingClasses.Length.Should().Be(0);
        }

        [Fact(DisplayName = "Should find matching test methods for all shared features between implementations")]
        public void TestMethodMatchingTests()
        {
            var helper = new ConfirmMatchingTestHelper();
            
            var errorCount = 0;
            foreach (var postgreClassInfo in helper.PostgreClassInfo)
            {
                var matchingSqlServerClassInfo = helper.SqlServerClassInfo.First(x => x.ReferenceName == postgreClassInfo.ReferenceName);

                var postgreMethods = postgreClassInfo.Class.GetMethods().Select(x => x.Name).ToArray();
                var sqlServerMethods = matchingSqlServerClassInfo.Class.GetMethods().Select(x => x.Name).ToArray();

                var postgreMissingSqlServer = postgreMethods.Except(sqlServerMethods).ToArray();
                var sqlServerMissingPostgre = sqlServerMethods.Except(postgreMethods).ToArray();

                _output.WriteLine($"Test class: {postgreClassInfo}");
                foreach (var missing in postgreMissingSqlServer)
                {
                    errorCount++;
                    _output.WriteLine($"***missing method: {missing}");
                }

                _output.WriteLine($"Test class: {matchingSqlServerClassInfo}");
                foreach (var missing in sqlServerMissingPostgre)
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
