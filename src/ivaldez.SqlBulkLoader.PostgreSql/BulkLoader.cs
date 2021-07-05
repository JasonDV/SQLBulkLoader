using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ivaldez.Sql.SqlBulkLoader.Core;
using Npgsql;

namespace ivaldez.Sql.SqlBulkLoader.PostgreSql
{
    /// <summary>
    /// BulkLoader is a convention based wrapper for the SqlBulkCopy utility.
    /// The bulk loader will use the DTO properties names to generate a SqlBulkLoader call.
    /// </summary>
    public class BulkLoader : IBulkLoader
    {
        private readonly ISqlBulkLoadUtility _sqlBulkCopyUtility;

        /// <summary>
        /// Constructor that uses default SqlBulkCopyUtility class.
        /// </summary>
        public BulkLoader()
        {
            _sqlBulkCopyUtility = new SqlBulkLoadUtility();
        }

        /// <summary>
        /// Constructor with SQL BulkCopy Utility. Interface allows for extension and testing. 
        /// </summary>
        /// <param name="sqlBulkCopyUtility"></param>
        public BulkLoader(ISqlBulkLoadUtility sqlBulkCopyUtility)
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
            var targetProperties = ReflectionHelper.GetTargetProperties<T>(propertiesToIgnore, renameFields);
            
            if (noBatch)
            {
                BulkCopyWithNoBatching(tableName, conn, dataToInsert, targetProperties);
            }
            else
            {
                BulkCopyWithBatching(tableName, conn, dataToInsert, batchSize, targetProperties);
            }
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
    }
}