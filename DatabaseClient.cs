using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Linq;

namespace SmolyShortener
{
    public class DatabaseClient : IDisposable
    {
        private SQLiteConnection _connection;

        public DatabaseClient(string file)
        {
            if (!File.Exists(file))
                SQLiteConnection.CreateFile(file);

            string connection = new SQLiteConnectionStringBuilder
            {
                DataSource = file,
                FailIfMissing = true
            }.ToString();
            _connection = new SQLiteConnection(connection);
            _connection.Open();
        }

        public async Task CreateTable(string table, params string[] columns)
        {
            var command = new SQLiteCommand(
                $"create table {table}({string.Join(',', columns)})",
                _connection);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> Write(string table, params (string column, string value)[] insertInfo)
        {
            for (int i = 0; i < insertInfo.Length; i++)
                insertInfo[i].value = string.Join(string.Empty, insertInfo[i].value.Prepend('\'').Append('\''));

            string columns = string.Join(',', insertInfo.Select(info => info.column));
            string values = string.Join(',', insertInfo.Select(info => info.value));

            var command = new SQLiteCommand(
                $"insert into {table}({columns}) " +
                $"values({values})",
                _connection);

            return await command.ExecuteNonQueryAsync();
        }

        public async IAsyncEnumerable<object[]> Read(
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

            var command = new SQLiteCommand(
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
        /// Tests the connection by running a basic SQL version command through the database and returns the result.
        /// </summary>
        /// <returns>
        /// Returns either the database version or the exception output if one occurred.
        /// </returns>
        public async Task<string> TestConnection()
        {
            var command = new SQLiteCommand("select SQLITE_VERSION()", _connection);
            string res;
            try
            {
                res = (await command.ExecuteScalarAsync()).ToString();
            }
            catch (System.Data.Common.DbException exc)
            {
                return exc.Message;
            }

            return res;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}