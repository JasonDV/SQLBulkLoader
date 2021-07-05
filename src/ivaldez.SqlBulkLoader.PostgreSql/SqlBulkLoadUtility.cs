using System.Collections.Generic;
using System.Linq;
using ivaldez.Sql.SqlBulkLoader.Core;
using Npgsql;

namespace ivaldez.Sql.SqlBulkLoader.PostgreSql
{
    /// <summary>
    /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
    /// the basic class can be extended if necessary, as well as for testing.
    /// </summary>
    public class SqlBulkLoadUtility: ISqlBulkLoadUtility
    {
        public void BulkCopy<T>(string tableName, NpgsqlConnection conn,
            TargetProperty[] targetProperties, IEnumerable<T> toInsert)
        {
            var propertyList = new List<string>();
            foreach (var property in targetProperties)
            {
                propertyList.Add(property.Name.ToLower());
            }

            var columnListString = string.Join(",", propertyList.Select(x => @"""" + x + @""""));
                
            using (var writer = conn
                .BeginBinaryImport($@"COPY ""{tableName}"" ({columnListString}) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var dto in toInsert)
                {
                    writer.StartRow();

                    foreach (var property in targetProperties)
                    {
                        writer.Write(property.PropertyInfo.GetValue(dto));
                    }
                }
                
                writer.Complete();
            }
        }
    }
}