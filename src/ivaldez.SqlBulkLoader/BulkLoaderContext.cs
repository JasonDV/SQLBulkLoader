using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;

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
            var name = GetName(expression);

            _renameFields.Add(name, newName);

            return this;
        }

        public BulkLoaderContext<T> Without(Expression<Func<T, object>> expression)
        {
            var name = GetName(expression);

            _withoutMembers.Add(name);

            return this;
        }

        public BulkLoaderContext<T> Without(string name)
        {
            _withoutMembers.Add(name);

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
                _batchSize);
        }

        public IReadOnlyDictionary<string, string> GetRenameRules()
        {
            return _renameFields;
        }

        public void SetBatchSize(int value)
        {
            _batchSize = value;
        }

        private string GetName(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                var ubody = (UnaryExpression) expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            var name = body.Member.Name;

            return name;
        }
    }
}