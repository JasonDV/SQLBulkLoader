using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ivaldez.Sql.IntegrationTests.Data
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
            var sql = @"SELECT * FROM dbo.Sample";

            return _testingDatabaseService.Query<SampleSurrogateKey>(sql);
        }
        
        public void ExecuteWithConnection(Action<SqlConnection> action)
        {
            _testingDatabaseService.WithConnection(action);
        }

        public void CreateSingleSurrogateKeyTable()
        {
            var sql = @"
CREATE TABLE dbo.Sample(
    Pk INT IDENTITY(1,1) PRIMARY KEY,
    TextValue nvarchar(200) NULL,
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
DROP TABLE dbo.Sample;
";

                _testingDatabaseService.Execute(sql);
            }
            catch {}
        }
    }
}