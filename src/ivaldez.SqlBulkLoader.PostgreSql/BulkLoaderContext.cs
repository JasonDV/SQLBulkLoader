using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Npgsql;

namespace ivaldez.SqlBulkLoader.PostgreSql
{
    public class BulkLoaderContext<T>
    {
        private readonly IBulkLoader _bulkLoader;
        private readonly NpgsqlConnection _conn;
        private readonly IEnumerable<T> _dataToInsert;
        private readonly bool _keepIdentityColumnValue;
        private readonly Dictionary<string, string> _renameFields;
        private readonly string _tableName;

        private string _identityOrSerialColumn;
        private readonly List<string> _withoutMembers;
        private int _batchSize;
        private bool _noBatch;

        public BulkLoaderContext(
            IBulkLoader bulkLoader,
            string tableName,
            NpgsqlConnection conn,
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

        public BulkLoaderContext<T> IdentityColumn(Expression<Func<T, object>> expression)
        {
            var name = GetName(expression);

            _identityOrSerialColumn = name;

            return this;
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
            if (_keepIdentityColumnValue && _identityOrSerialColumn == null)
            {
                throw new ArgumentException($@"method ""{nameof(IdentityColumn)}"" must be called when ""keepIdentityColumnValue"" is True.");
            }


            var propertiesToIgnore = _withoutMembers.ToList();

            if (_keepIdentityColumnValue == false && _identityOrSerialColumn != null)
            {
                propertiesToIgnore.Add(_identityOrSerialColumn);
            }

            _bulkLoader.Insert(
                _tableName,
                _conn,
                _keepIdentityColumnValue,
                _dataToInsert,
                propertiesToIgnore,
                _renameFields,
                _batchSize,
                _noBatch);
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