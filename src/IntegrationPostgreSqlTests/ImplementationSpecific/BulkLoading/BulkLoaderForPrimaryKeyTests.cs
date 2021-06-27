using System;
using FluentAssertions;
using ivaldez.Sql.IntegrationPostgreSqlTests.Data;
using ivaldez.SqlBulkLoader.PostgreSql;
using SharedTestFramework;
using Xunit;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.ImplementationSpecific.BulkLoading
{
    [ImplementationSpecificTest]
    public class BulkLoaderForPrimaryKeyTests
    {
        [Fact]
        public void ShouldThrowErrorIfIdentityValueIsNotSupplied()
        {
            var testingDatabaseService = new TestingDatabaseService();
            testingDatabaseService.CreateTestDatabase();

            var dataGateway = new TestingDataGateway(testingDatabaseService);

            dataGateway.DropTable();
            dataGateway.CreateSingleSurrogateKeyTable();

            var dtos = new[]
            {
                new SampleSurrogateKey
                {
                    Pk = 100,
                    TextValue = "JJ",
                    IntValue = 100,
                    DecimalValue = 100.99m
                }
            };

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                dataGateway.ExecuteWithConnection(conn =>
                {
                    BulkLoaderFactory.Create()
                        .InsertWithOptions("sample", conn, true, dtos)
                        .Execute();
                });
            });

            exception.Message.Should().Contain(@"must be called when ""keepIdentityColumnValue"" is True.");
        }
    }
}