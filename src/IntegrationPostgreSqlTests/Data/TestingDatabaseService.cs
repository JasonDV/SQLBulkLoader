using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Npgsql;

namespace ivaldez.Sql.IntegrationPostgreSqlTests.Data
{
    public class TestingDatabaseService
    {
        private static readonly string DatabaseName = "iValdezTest".ToLower();
        private readonly string _connectionString = $"Host=localhost;Username=postgres;Password=example;Database={DatabaseName};";
        private readonly string _connectionStringMaster = $"Host=localhost;Username=postgres;Password=example;Database=postgres;";

        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
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
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var result = conn.Execute(sql, dtos);

                return result;
            }
        }

        public int Execute(string sql, object param = null)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                var result = conn.Execute(sql, param);

                return result;
            }
        }

        public void WithConnection(Action<NpgsqlConnection> action)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                action(conn);
                conn.Close();
            }
        }

        public void CreateTestDatabase()
        {
            using (var connection = new NpgsqlConnection(_connectionStringMaster))
            {
                connection.Open();
                bool result;
                using (var command2 = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname='{DatabaseName}'", connection))
                {
                    result = (command2.ExecuteScalar() != null);
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