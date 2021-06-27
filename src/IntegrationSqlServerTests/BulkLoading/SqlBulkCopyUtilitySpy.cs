using System.Collections.Generic;
using System.Data.SqlClient;
using ivaldez.Sql.SqlBulkLoader;

namespace ivaldez.Sql.IntegrationSqlServerTests.BulkLoading
{
    public class SqlBulkCopyUtilitySpy: BulkLoader.ISqlBulkCopyUtility
    {
        public void BulkCopy<T>(string tableName,
            SqlConnection conn, 
            SqlBulkCopyOptions options,
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

        public int BulkCopyCalled { get; set; }
    }
}