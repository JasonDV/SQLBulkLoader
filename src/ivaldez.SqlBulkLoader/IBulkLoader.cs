﻿using System.Collections.Generic;
using System.Data.SqlClient;

namespace ivaldez.Sql.SqlBulkLoader
{
    /// <summary>
    /// BulkLoader is a convention based wrapper for the SqlBulkCopy utility.
    /// The bulk loader will use the DTO properties names to generate a SqlBulkLoader call.
    /// </summary>
    public interface IBulkLoader
    {
        void Insert<T>(
            string tableName,
            SqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            int batchSize = 5000,
            bool noBatch = false);

        BulkLoaderContext<T> InsertWithOptions<T>(
            string tableName,
            SqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            int batchSize = 5000);

        void Insert<T>(string tableName,
            SqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            List<string> propertiesToIgnore,
            Dictionary<string, string> renameFields,
            int batchSize = 5000,
            bool noBatch = false);
    }
}