using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using FastMember;

namespace ivaldez.Sql.SqlBulkLoader
{
    /// <summary>
    /// BulkLoader is a convention based wrapper for the SqlBulkCopy utility.
    /// The bulk loader will use the DTO properties names to generate a SqlBulkLoader call.
    /// </summary>
    public class BulkLoader : IBulkLoader
    {
        private readonly ISqlBulkCopyUtility _sqlBulkCopyUtility;

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
            SqlConnection conn,
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
            SqlConnection conn,
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
            SqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            List<string> propertiesToIgnore,
            Dictionary<string, string> renameFields,
            int batchSize = 5000, 
            bool noBatch = false)
        {
            var targetProperties = GetTargetProperties<T>(propertiesToIgnore, renameFields);

            var options = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.TableLock;

            if (keepIdentityColumnValue)
            {
                options |= SqlBulkCopyOptions.KeepIdentity;
            }

            if (noBatch)
            {
                BulkCopyWithNoBatching(tableName, conn, dataToInsert, options, targetProperties);
            }
            else
            {
                BulkCopyWithBatching(tableName, conn, dataToInsert, batchSize, options, targetProperties);
            }
        }

        private void BulkCopyWithNoBatching<T>(string tableName, SqlConnection conn, IEnumerable<T> dataToInsert,
            SqlBulkCopyOptions options, TargetProperty[] targetProperties)
        {
            _sqlBulkCopyUtility.BulkCopy(tableName, conn, options, targetProperties, dataToInsert);
        }

        private void BulkCopyWithBatching<T>(string tableName, SqlConnection conn, IEnumerable<T> dataToInsert, int batchSize,
            SqlBulkCopyOptions options, TargetProperty[] targetProperties)
        {
            var batch = new List<T>(batchSize);

            foreach (var item in dataToInsert)
            {
                batch.Add(item);

                if (batch.Count >= batchSize)
                {
                    _sqlBulkCopyUtility.BulkCopy(tableName, conn, options, targetProperties, batch);
                    batch.Clear();
                }
            }

            if (batch.Any())
            {
                _sqlBulkCopyUtility.BulkCopy(tableName, conn, options, targetProperties, batch);
                batch.Clear();
            }
        }

        public class SqlBulkCopyUtility: ISqlBulkCopyUtility
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

        public interface ISqlBulkCopyUtility
        {
            void BulkCopy<T>(string tableName, SqlConnection conn, SqlBulkCopyOptions options,
                TargetProperty[] targetProperties, IEnumerable<T> toInsert);
        }

        public class TargetProperty
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public string OriginalName { get; set; }
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

    public class BulkLoaderFactory
    {
        public static IBulkLoader Create()
        {
            return new BulkLoader(new BulkLoader.SqlBulkCopyUtility());
        }
    }
}