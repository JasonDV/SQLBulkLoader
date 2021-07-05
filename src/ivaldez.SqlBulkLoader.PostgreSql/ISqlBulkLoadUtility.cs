using System.Collections.Generic;
using ivaldez.Sql.SqlBulkLoader.Core;
using Npgsql;

namespace ivaldez.Sql.SqlBulkLoader.PostgreSql
{
    /// <summary>
    /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
    /// the basic class can be extended if necessary, as well as for testing.
    /// </summary>
    public interface ISqlBulkLoadUtility
    {
        void BulkCopy<T>(string tableName, NpgsqlConnection conn,
            TargetProperty[] targetProperties, IEnumerable<T> toInsert);
    }
}