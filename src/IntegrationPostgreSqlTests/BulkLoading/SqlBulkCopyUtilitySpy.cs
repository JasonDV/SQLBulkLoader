using System.Collections.Generic;
using ivaldez.Sql.SqlBulkLoader.Core;
using ivaldez.Sql.SqlBulkLoader.PostgreSql;
using Npgsql;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.BulkLoading
{
    public class SqlBulkCopyUtilitySpy: ISqlBulkLoadUtility
    {
        public int BulkCopyCalled { get; set; }

        public void BulkCopy<T>(string tableName,
            NpgsqlConnection conn, 
            TargetProperty[] targetProperties,
            IEnumerable<T> toInsert)
        {
            BulkCopyCalled++;

            new SqlBulkLoadUtility()
                .BulkCopy(tableName,
                    conn, 
                    targetProperties,
                    toInsert);
        }
    }
}