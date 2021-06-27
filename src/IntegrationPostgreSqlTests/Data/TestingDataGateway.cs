using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Npgsql;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.Data
{
    public class TestingDataGateway
    {
        private readonly TestingDatabaseService _testingDatabaseService;

        public TestingDataGateway(TestingDatabaseService testingDatabaseService)
        {
            _testingDatabaseService = testingDatabaseService;
        }

        public IEnumerable<SampleSurrogateKey> GetAllSampleSurrogateKey()
        {
            var sql = @"SELECT * FROM sample";

            return _testingDatabaseService.Query<SampleSurrogateKey>(sql);
        }
        
        public void ExecuteWithConnection(Action<NpgsqlConnection> action)
        {
            _testingDatabaseService.WithConnection(action);
        }

        public void CreateSingleSurrogateKeyTable()
        {
            var sql = @"
CREATE TABLE sample(
    Pk SERIAL PRIMARY KEY,
    TextValue varchar(200) NULL,
    IntValue int NULL,
    DecimalValue decimal(18,8) NULL
)
";

            _testingDatabaseService.Execute(sql);
        }

        public void DropTable()
        {
            try
            {
                var sql = @"
DROP TABLE sample;
";

                _testingDatabaseService.Execute(sql);
            }
            catch {}
        }
    }
}