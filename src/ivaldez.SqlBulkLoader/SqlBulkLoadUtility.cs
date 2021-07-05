using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FastMember;
using ivaldez.Sql.SqlBulkLoader.Core;

namespace ivaldez.Sql.SqlBulkLoader
{
    /// <summary>
    /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
    /// the basic class can be extended if necessary, as well as for testing.
    /// </summary>
    public class SqlBulkLoadUtility: ISqlBulkLoadUtility
    {
        public void BulkCopy<T>(string tableName, SqlConnection conn, SqlBulkCopyOptions options,
            TargetProperty[] targetProperties, IEnumerable<T> toInsert)
        {
            using (var bulkCopy = new SqlBulkCopy(conn, options, null))
            {
                var parameters = targetProperties.Select(x => x.OriginalName).ToArray();

                using (var reader = ObjectReader.Create(toInsert, parameters))
                {
                    foreach (var property in targetProperties)
                    {
                        bulkCopy.ColumnMappings.Add(property.OriginalName, property.Name);
                    }

                    bulkCopy.BulkCopyTimeout = 900;
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.WriteToServer(reader);

                    bulkCopy.Close();
                }
            }
        }
    }
}