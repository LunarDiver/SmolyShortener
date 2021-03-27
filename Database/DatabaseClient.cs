using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Linq;

namespace SmolyShortener.Database
{
    public class DatabaseClient : IDisposable
    {
        private SqliteConnection _connection;

        public DatabaseClient(string file)
        {
            // if (!File.Exists(file))
            //     File.Create(file).Dispose();

            string connection = new SqliteConnectionStringBuilder
            {
                DataSource = file,
            }.ToString();
            _connection = new SqliteConnection(connection);
            _connection.Open();
        }

        public async Task CreateTableAsync(string table, params DbColumn[] columns)
        {
            var command = new SqliteCommand(
                $"create table {table}({string.Join(',', columns)})",
                _connection);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> TableExistsAsync(string table)
        {
            var command = new SqliteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}'", _connection);

            return await command.ExecuteScalarAsync() != null;
        }

        public async Task<int> WriteAsync(string table, params (string column, string value)[] insertInfo)
        {
            for (int i = 0; i < insertInfo.Length; i++)
                insertInfo[i].value = string.Join(string.Empty, insertInfo[i].value.Prepend('\'').Append('\''));

            string columns = string.Join(',', insertInfo.Select(info => info.column));
            string values = string.Join(',', insertInfo.Select(info => info.value));

            var command = new SqliteCommand(
                $"insert into {table}({columns}) " +
                $"values({values})",
                _connection);

            return await command.ExecuteNonQueryAsync();
        }

        public async IAsyncEnumerable<object[]> ReadAsync(
            string table,
            IEnumerable<string> columns,
            int maxRows,
            IEnumerable<string> conditions = null,
            bool unique = false)
        {
            string colsString = string.Join(',', columns);
            string condsString = conditions != null
                ? $"where {string.Join(" and ", conditions)} "
                : "";

            var command = new SqliteCommand(
                $"select {(unique ? "distinct " : "")}{colsString} from {table} " +
                condsString +
                $"limit {maxRows}",
                _connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new object[reader.FieldCount];
                reader.GetValues(row);
                yield return row;
            }
        }

        /// <summary>
        /// Tests the connection by running a basic SQL version command through the database.
        /// </summary>
        /// <returns>
        /// Returns whether the command was successful.
        /// </returns>
        public async Task<bool> TestConnectionAsync()
        {
            var command = new SqliteCommand("select SQLITE_VERSION()", _connection);
            try
            {
                return await command.ExecuteScalarAsync() != null;
            }
            catch (System.Data.Common.DbException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}