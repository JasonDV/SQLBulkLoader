﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ivaldez.Sql.IntegrationTests.Data;
using ivaldez.Sql.SqlBulkLoader;
using Xunit;

namespace ivaldez.Sql.IntegrationTests.BulkLoading
{
    public class BulkLoaderForPrimaryKeyTests
    {
        [Fact]
        public void ShouldInsertPrimaryKeyWhenKeepIdentityOptionIsTrue()
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
                    .Execute();
            });

            var databaseDtos = dataGateway.GetAllSampleSurrogateKey().ToArray();

            var firstDto = databaseDtos.First(x => x.TextValue == "JJ");
            firstDto.Pk.Should().Be(100);
            firstDto.IntValue.Should().Be(100);
            firstDto.DecimalValue.Should().Be(100.99m);

            var secondDto = databaseDtos.First(x => x.TextValue == "ZZ");
            secondDto.Pk.Should().Be(200);
            secondDto.IntValue.Should().Be(999);
            secondDto.DecimalValue.Should().Be(123.45m);
        }

        [Fact]
        public void ShouldNotInsertPrimaryKeyWhenKeepIdentityOptionIsFalse()
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
                    .InsertWithOptions("Sample", conn, false, dtos)
                    .Execute();
            });

            var databaseDtos = dataGateway.GetAllSampleSurrogateKey().ToArray();

            var firstDto = databaseDtos.First(x => x.TextValue == "JJ");
            firstDto.Pk.Should().NotBe(100);
            firstDto.IntValue.Should().Be(100);
            firstDto.DecimalValue.Should().Be(100.99m);

            var secondDto = databaseDtos.First(x => x.TextValue == "ZZ");
            secondDto.Pk.Should().NotBe(200);
            secondDto.IntValue.Should().Be(999);
            secondDto.DecimalValue.Should().Be(123.45m);
        }
    }
}
