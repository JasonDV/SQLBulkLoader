using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

namespace ivaldez.SqlBulkLoader.PostgreSql
{
    /// <summary>
    /// BulkLoader is a convention based wrapper for the SqlBulkCopy utility.
    /// The bulk loader will use the DTO properties names to generate a SqlBulkLoader call.
    /// </summary>
    public class BulkLoader : IBulkLoader
    {
        private readonly ISqlBulkCopyUtility _sqlBulkCopyUtility;

        /// <summary>
        /// Constructor that uses default SqlBulkCopyUtility class.
        /// </summary>
        public BulkLoader()
        {
            _sqlBulkCopyUtility = new SqlBulkCopyUtility();
        }

        /// <summary>
        /// Constructor with SQL BulkCopy Utility. Interface allows for extension and testing. 
        /// </summary>
        /// <param name="sqlBulkCopyUtility"></param>
        public BulkLoader(ISqlBulkCopyUtility sqlBulkCopyUtility)
        {
            _sqlBulkCopyUtility = sqlBulkCopyUtility;
        }

        /// <summary>
        /// Bulk load with customizations.
        /// </summary>
        /// <returns>A context object used to customize the bulk load operation.</returns>
        /// <typeparam name="T">Generic DTO type.</typeparam>
        /// <param name="tableName">The target table for the bulk copy</param>
        /// <param name="conn">A SQL connection</param>
        /// <param name="keepIdentityColumnValue">If true, the bulk copy will attempt to insert into identity columns on the target table.</param>
        /// <param name="dataToInsert">The data that will be bulk loaded into the target table.</param>
        /// <param name="batchSize">The batch size of each bulk load.</param>
        /// <returns></returns>
        public BulkLoaderContext<T> InsertWithOptions<T>(
            string tableName,
            NpgsqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            int batchSize = 5000)
        {
            return new BulkLoaderContext<T>(
                this,
                tableName,
                conn,
                keepIdentityColumnValue,
                dataToInsert,
                batchSize);
        }

        /// <summary>
        /// Simple insert method for a DTO that matches the target table.
        /// </summary>
        /// <typeparam name="T">Generic DTO type.</typeparam>
        /// <param name="tableName">The target table for the bulk copy</param>
        /// <param name="conn">A SQL connection</param>
        /// <param name="keepIdentityColumnValue">If true, the bulk copy will attempt to insert into identity columns on the target table.</param>
        /// <param name="dataToInsert">The data that will be bulk loaded into the target table.</param>
        /// <param name="batchSize">The batch size of each bulk load.</param> 
        /// <param name="noBatch">Indicates that no batching should occur and all data should be written at once.</param>
        public void Insert<T>(
            string tableName,
            NpgsqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            int batchSize = 5000,
            bool noBatch = false)
        {
            Insert(
                tableName,
                conn,
                keepIdentityColumnValue,
                dataToInsert,
                new List<string>(),
                new Dictionary<string, string>(),
                batchSize,
                noBatch);
        }

        /// <summary>
        /// The base insert method for bulk loading.
        /// </summary>
        /// <typeparam name="T">Generic DTO type.</typeparam>
        /// <param name="tableName">The target table for the bulk copy</param>
        /// <param name="conn">A SQL connection</param>
        /// <param name="keepIdentityColumnValue">If true, the bulk copy will attempt to insert into identity columns on the target table.</param>
        /// <param name="dataToInsert">The data that will be bulk loaded into the target table.</param>
        /// <param name="propertiesToIgnore">A list of properties on the DTO to ignore.</param>
        /// <param name="renameFields">Pairs of source property names and mapped column names on the target table.</param>
        /// <param name="batchSize">The batch size of each bulk load.</param>
        /// <param name="noBatch">Indicates that no batching should occur and all data should be written at once.</param>
        public void Insert<T>(string tableName,
            NpgsqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            List<string> propertiesToIgnore,
            Dictionary<string, string> renameFields,
            int batchSize = 5000, 
            bool noBatch = false)
        {
            var targetProperties = GetTargetProperties<T>(propertiesToIgnore, renameFields);
            
            if (noBatch)
            {
                BulkCopyWithNoBatching(tableName, conn, dataToInsert, targetProperties);
            }
            else
            {
                BulkCopyWithBatching(tableName, conn, dataToInsert, batchSize, targetProperties);
            }
        }

        /// <summary>
        /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
        /// the basic class can be extended if necessary, as well as for testing.
        /// </summary>
        public class SqlBulkCopyUtility: ISqlBulkCopyUtility
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

        /// <summary>
        /// SqlBulkCopyUtility wraps the basic functionality for bulkCopy. The interface is provided so that
        /// the basic class can be extended if necessary, as well as for testing.
        /// </summary>
        public interface ISqlBulkCopyUtility
        {
            void BulkCopy<T>(string tableName, NpgsqlConnection conn,
                TargetProperty[] targetProperties, IEnumerable<T> toInsert);
        }

        public class TargetProperty
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public string OriginalName { get; set; }
        }

        private void BulkCopyWithNoBatching<T>(string tableName, NpgsqlConnection conn, IEnumerable<T> dataToInsert,
            TargetProperty[] targetProperties)
        {
            _sqlBulkCopyUtility.BulkCopy(tableName, conn, targetProperties, dataToInsert);
        }

        private void BulkCopyWithBatching<T>(string tableName, NpgsqlConnection conn, IEnumerable<T> dataToInsert, int batchSize,
            TargetProperty[] targetProperties)
        {
            var batch = new List<T>(batchSize);

            foreach (var item in dataToInsert)
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    _sqlBulkCopyUtility.BulkCopy(tableName, conn, targetProperties, batch);
                    batch.Clear();
                }
            }

            if (batch.Any())
            {
                _sqlBulkCopyUtility.BulkCopy(tableName, conn, targetProperties, batch);
                batch.Clear();
            }
        }

        private static TargetProperty[] GetTargetProperties<T>(List<string> propertiesToIgnore,
            Dictionary<string, string> renameFields)
        {
            var ignoreProperties = new HashSet<string>(propertiesToIgnore);

            var targetProperties = typeof(T)
                .GetProperties()
                .Where(x => ignoreProperties.Contains(x.Name) == false)
                .Select(x =>
                {
                    var fieldName = x.Name;

                    if (renameFields.ContainsKey(fieldName))
                    {
                        fieldName = renameFields[fieldName];
                    }

                    return new TargetProperty
                    {
                        Name = fieldName,
                        OriginalName = x.Name,
                        Type = x.PropertyType,
                        PropertyInfo = x
                    };
                }).ToArray();

            return targetProperties;
        }
    }

    /// <summary>
    /// Factory class used to create default instances of the BulkLoader
    /// </summary>
    public class BulkLoaderFactory
    {
        public static IBulkLoader Create()
        {
            return new BulkLoader(new BulkLoader.SqlBulkCopyUtility());
        }
    }
}