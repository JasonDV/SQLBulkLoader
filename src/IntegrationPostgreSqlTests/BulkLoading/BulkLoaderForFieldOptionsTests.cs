﻿using System.Linq;
using FluentAssertions;
using ivaldez.Sql.IntegrationPostgreSqlTests.Data;
using ivaldez.Sql.SqlBulkLoader.PostgreSql;
using Xunit;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading
{
    public class BulkLoaderForFieldOptionsTests
    {
        
        [Fact]
        public void ShouldRespectRenamedFields()
        {
            var testingDatabaseService = new TestingDatabaseService();
            testingDatabaseService.CreateTestDatabase();

            var dataGateway = new TestingDataGateway(testingDatabaseService);

            dataGateway.DropTable();
            dataGateway.CreateSingleSurrogateKeyTable();

            var dtos = new[]
            {
                new SampleSurrogateKeyDifferentNamesDto
                {
                    Pk = 100,
                    TextValueExtra = "JJ",
                    IntValueExtra = 100,
                    DecimalValueExtra = 100.99m
                },
                new SampleSurrogateKeyDifferentNamesDto
                {
                    Pk = 200,
                    TextValueExtra = "ZZ",
                    IntValueExtra = 999,
                    DecimalValueExtra = 123.45m
                }
            };

            dataGateway.ExecuteWithConnection(conn =>
            {
                BulkLoaderFactory.Create()
                    .InsertWithOptions("sample", conn, true, dtos)
                    .IdentityColumn(c => c.Pk)
                    .With(c => c.TextValueExtra, "TextValue")
                    .With(c => c.IntValueExtra, "IntValue")
                    .With(c => c.DecimalValueExtra, "DecimalValue")
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
        public void ShouldRespectTheWithoutOption()
        {
            var testingDatabaseService = new TestingDatabaseService();
            testingDatabaseService.CreateTestDatabase();

            var dataGateway = new TestingDataGateway(testingDatabaseService);

            dataGateway.DropTable();
            dataGateway.CreateSingleSurrogateKeyTable();

            var dtos = new[]
            {
                new SampleSurrogateKey()
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
                    .InsertWithOptions("sample", conn, true, dtos)
                    .IdentityColumn(c => c.Pk)
                    .Without("DecimalValue")
                    .Without(t => t.IntValue)
                    .Execute();
            });

            var databaseDtos = dataGateway.GetAllSampleSurrogateKey().ToArray();

            var firstDto = databaseDtos.First(x => x.TextValue == "JJ");
            firstDto.Pk.Should().Be(100);
            firstDto.IntValue.Should().BeNull();
            firstDto.DecimalValue.Should().BeNull();

            var secondDto = databaseDtos.First(x => x.TextValue == "ZZ");
            secondDto.Pk.Should().Be(200);
            secondDto.IntValue.Should().BeNull();
            secondDto.DecimalValue.Should().BeNull();
        }

        [Fact]
        public void ShouldHaveAccessToRenameRules()
        {
            var testingDatabaseService = new TestingDatabaseService();
            testingDatabaseService.CreateTestDatabase();

            var dataGateway = new TestingDataGateway(testingDatabaseService);

            dataGateway.DropTable();
            dataGateway.CreateSingleSurrogateKeyTable();

            var dtos = new[]
            {
                new SampleSurrogateKeyDifferentNamesDto
                {
                    Pk = 100,
                    TextValueExtra = "JJ",
                    IntValueExtra = 100,
                    DecimalValueExtra = 100.99m
                },
                new SampleSurrogateKeyDifferentNamesDto
                {
                    Pk = 200,
                    TextValueExtra = "ZZ",
                    IntValueExtra = 999,
                    DecimalValueExtra = 123.45m
                }
            };

            var bulkLoader = BulkLoaderFactory.Create()
                .InsertWithOptions("sample", null, true, dtos)
                .With(c => c.TextValueExtra, "TextValue")
                .With(c => c.IntValueExtra, "IntValue")
                .With(c => c.DecimalValueExtra, "DecimalValue");

            var renameRules = bulkLoader.GetRenameRules();
            renameRules.Keys.Count().Should().Be(3);
            renameRules.First(x => x.Key == "TextValueExtra").Value.Should().Be("TextValue");
            renameRules.First(x => x.Key == "IntValueExtra").Value.Should().Be("IntValue");
            renameRules.First(x => x.Key == "DecimalValueExtra").Value.Should().Be("DecimalValue");
        }
    }
}
