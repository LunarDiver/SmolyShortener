using System;
using System.IO;
using System.Data.SQLite;
using System.Threading.Tasks;

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