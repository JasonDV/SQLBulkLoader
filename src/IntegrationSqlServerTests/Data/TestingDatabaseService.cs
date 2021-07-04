using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using ivaldez.Sql.SharedTestFramework;

namespace ivaldez.Sql.IntegrationSqlServerTests.Data
{
    public class TestingDatabaseService
    {
        public TestingDatabaseService()
        {
            _connectionString = LocalConfig.Instance.SqlExpressConnectionString;
            _connectionStringMaster = LocalConfig.Instance.SqlExpressMasterConnectionString;
            _databaseName = LocalConfig.Instance.DatabaseName;
        }

        private readonly string _databaseName;
        private readonly string _connectionString;
        private readonly string _connectionStringMaster;

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var results = conn.Query<T>(sql, param);

                foreach (var dto in results)
                {
                    yield return dto;
                }
            }
        }

        public int Execute(string sql, object param = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var result = conn.Execute(sql, param);

                return result;
            }
        }

        public void WithConnection(Action<SqlConnection> action)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                action(conn);
                conn.Close();
            }
        }

        public void CreateTestDatabase()
        {
            using (var connection = new SqlConnection(_connectionStringMaster))
            {
                connection.Open();
                bool result;
                using (var command2 = new SqlCommand($"SELECT db_id('{_databaseName}')", connection))
                {
                    result = (command2.ExecuteScalar() != DBNull.Value);
                }

                if (result == false)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"CREATE DATABASE {_databaseName}";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}