using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace ivaldez.Sql.IntegrationSqlServerTests.Data
{
    public class TestingDatabaseService
    {
        private static readonly string DatabaseName = "iValdezTest";
        private readonly string _connectionString = $"Data Source=localhost;Initial Catalog={DatabaseName};Integrated Security=true;";
        private readonly string _connectionStringMaster = $"Data Source=localhost;Initial Catalog=master;Integrated Security=true;";

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

        public int Insert<T>(string sql, IEnumerable<T> dtos)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var result = conn.Execute(sql, dtos);

                return result;
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
                using (var command2 = new SqlCommand($"SELECT db_id('{DatabaseName}')", connection))
                {
                    result = (command2.ExecuteScalar() != DBNull.Value);
                }

                if (result == false)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"CREATE DATABASE {DatabaseName}";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}