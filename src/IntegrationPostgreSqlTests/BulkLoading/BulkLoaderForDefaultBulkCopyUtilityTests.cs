using System.Linq;
using FluentAssertions;
using ivaldez.Sql.IntegrationPostgreSqlTests.Data;
using ivaldez.Sql.SqlBulkLoader.PostgreSql;
using Xunit;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading
{
    public class BulkLoaderForDefaultBulkCopyUtilityTests
    {
        [Fact]
        public void ShouldUseDefaultBulkCopyUtilityWhenNoConstructorParams()
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

            dataGateway.ExecuteWithConnection(conn =>
            {
                new BulkLoader()
                    .InsertWithOptions("sample", conn, true, dtos)
                    .IdentityColumn(c => c.Pk)
                    .Execute();
            });

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