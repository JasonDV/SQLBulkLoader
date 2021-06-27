using System.Collections.Generic;
using ivaldez.SqlBulkLoader.PostgreSql;
using Npgsql;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading
{
    public class SqlBulkCopyUtilitySpy: BulkLoader.ISqlBulkCopyUtility
    {
        public int BulkCopyCalled { get; set; }

        public void BulkCopy<T>(string tableName,
            NpgsqlConnection conn, 
            BulkLoader.SqlBulkCopyOptions options,
            BulkLoader.TargetProperty[] targetProperties,
            IEnumerable<T> toInsert)
        {
            BulkCopyCalled++;

            new BulkLoader.SqlBulkCopyUtility()
                .BulkCopy(tableName,
                    conn, 
                    options,
                    targetProperties,
                    toInsert);
        }
    }
}