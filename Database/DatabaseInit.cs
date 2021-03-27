using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmolyShortener.Database
{
    public static class DatabaseInit
    {
        public static IReadOnlyDictionary<string, DbColumn[]> Tables { get; } = new Dictionary<string, DbColumn[]>()
        {
            {
                "shorten",
                new[]
                {
                    new DbColumn("id", DbDatatype.TEXT, true, false),
                    new DbColumn("redirect", DbDatatype.TEXT, "/", false, false),
                    new DbColumn("expiry", DbDatatype.TEXT, false, true)
                }
            }
        };

        public static async Task<DatabaseClient> ConnectDatabase(string dbfile)
        {
            var client = new DatabaseClient(dbfile);

            if (!(await client.TestConnectionAsync()))
                throw new Microsoft.Data.Sqlite.SqliteException("Connection test was not successful.", -1);

            foreach (var table in Tables)
                if (!(await client.TableExistsAsync(table.Key)))
                    await client.CreateTableAsync(table.Key, table.Value);

            return client;
        }
    }
}