using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using ivaldez.Sql.IntegrationTests.Data;
using ivaldez.Sql.SqlBulkLoader;
using Xunit;

namespace ivaldez.Sql.IntegrationTests.BulkLoading
{
    public class BulkLoaderForNoBatchTests
    {
        [Fact]
        public void ShouldNotBatchWhenOptionSelected()
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
                },
                new SampleSurrogateKey
                {
                    Pk = 200,
                    TextValue = "ZZ",
                    IntValue = 999,
                    DecimalValue = 123.45m
                }
            };

            var sqlBulkCopyUtilitySpy = new SqlBulkCopyUtilitySpy();
            
            dataGateway.ExecuteWithConnection(conn =>
            {
                new BulkLoader(sqlBulkCopyUtilitySpy)
                    .InsertWithOptions("Sample", conn, true, dtos)
                    .SetBatchSize(1)
                    .NoBatch()
                    .Execute();
            });

            sqlBulkCopyUtilitySpy.BulkCopyCalled.Should().Be(1);

            var databaseDtos = dataGateway.GetAllSampleSurrogateKey().ToArray();

            var firstDto = databaseDtos.First(x => x.TextValue == "JJ");
            firstDto.IntValue.Should().Be(100);
            firstDto.DecimalValue.Should().Be(100.99m);

            var secondDto = databaseDtos.First(x => x.TextValue == "ZZ");
            secondDto.IntValue.Should().Be(999);
            secondDto.DecimalValue.Should().Be(123.45m);
        }
    }
}
