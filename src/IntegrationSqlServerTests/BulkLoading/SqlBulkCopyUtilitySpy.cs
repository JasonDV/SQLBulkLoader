using System.Collections.Generic;
using System.Data.SqlClient;
using ivaldez.Sql.SqlBulkLoader;
using ivaldez.Sql.SqlBulkLoader.Core;

namespace ivaldez.Sql.IntegrationSqlServerTests.BulkLoading
{
    public class SqlBulkCopyUtilitySpy: ISqlBulkLoadUtility
    {
        public void BulkCopy<T>(string tableName,
            SqlConnection conn, 
            SqlBulkCopyOptions options,
            TargetProperty[] targetProperties,
            IEnumerable<T> toInsert)
        {
            BulkCopyCalled++;

            new SqlBulkLoadUtility()
                .BulkCopy(tableName,
                    conn, 
                    options,
                    targetProperties,
                    toInsert);
        }

        public int BulkCopyCalled { get; set; }
    }
}