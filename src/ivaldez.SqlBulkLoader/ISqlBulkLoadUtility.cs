using System.Collections.Generic;
using System.Data.SqlClient;
using ivaldez.Sql.SqlBulkLoader.Core;

namespace ivaldez.Sql.SqlBulkLoader
{
    /// <summary>
    /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
    /// the basic class can be extended if necessary, as well as for testing.
    /// </summary>
    public interface ISqlBulkLoadUtility
    {
        void BulkCopy<T>(string tableName, SqlConnection conn, SqlBulkCopyOptions options,
            TargetProperty[] targetProperties, IEnumerable<T> toInsert);
    }
}