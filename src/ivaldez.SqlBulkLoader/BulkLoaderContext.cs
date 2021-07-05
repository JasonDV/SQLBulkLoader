using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using ivaldez.Sql.SqlBulkLoader.Core;

namespace ivaldez.Sql.SqlBulkLoader
{
    public class BulkLoaderContext<T>
    {
        private readonly IBulkLoader _bulkLoader;
        private readonly SqlConnection _conn;
        private readonly IEnumerable<T> _dataToInsert;
        private readonly bool _keepIdentityColumnValue;
        private readonly Dictionary<string, string> _renameFields;
        private readonly string _tableName;

        private readonly List<string> _withoutMembers;
        private int _batchSize;
        private bool _noBatch;

        public BulkLoaderContext(
            IBulkLoader bulkLoader,
            string tableName,
            SqlConnection conn,
            bool keepIdentityColumnValue,
            IEnumerable<T> dataToInsert,
            int batchSize)
        {
            _withoutMembers = new List<string>();
            _renameFields = new Dictionary<string, string>();
            _bulkLoader = bulkLoader;
            _tableName = tableName;
            _conn = conn;
            _keepIdentityColumnValue = keepIdentityColumnValue;
            _dataToInsert = dataToInsert;
            _batchSize = batchSize;
        }

        public BulkLoaderContext<T> With(Expression<Func<T, object>> expression, string newName)
        {
            var name = ExpressionHelper.GetName(expression);

            _renameFields.Add(name, newName);

            return this;
        }

        public BulkLoaderContext<T> Without(Expression<Func<T, object>> expression)
        {
            var name = ExpressionHelper.GetName(expression);

            _withoutMembers.Add(name);

            return this;
        }

        public BulkLoaderContext<T> Without(string name)
        {
            _withoutMembers.Add(name);

            return this;
        }

        public IReadOnlyDictionary<string, string> GetRenameRules()
        {
            return _renameFields;
        }

        public BulkLoaderContext<T> SetBatchSize(int value)
        {
            _batchSize = value;

            return this;
        }

        public BulkLoaderContext<T> NoBatch()
        {
            _noBatch = true;

            return this;
        }

        public void Execute()
        {
            _bulkLoader.Insert(
                _tableName,
                _conn,
                _keepIdentityColumnValue,
                _dataToInsert,
                _withoutMembers,
                _renameFields,
                _batchSize,
                _noBatch);
        }
    }
}