﻿using System.Linq;
using FluentAssertions;
using ivaldez.Sql.IntegrationSqlServerTests.Data;
using ivaldez.Sql.SqlBulkLoader;
using Xunit;

namespace ivaldez.Sql.IntegrationSqlServerTests.BulkLoading
{
    public class BulkLoaderGeneralTests
    {
        [Fact]
        public void ShouldBulkLoad()
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
                BulkLoaderFactory.Create()
                    .InsertWithOptions("Sample", conn, true, dtos)
                    .Without(c => c.Pk)
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