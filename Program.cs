using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SmolyShortener.Database;

namespace SmolyShortener
{
    public class Program
    {
        public static readonly string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            nameof(SmolyShortener));

        public static readonly string DbPath = Path.Combine(DataPath, "app.db");

        public static DatabaseClient DbClient { get; private set; }

        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += MainDispose;

            try
            {
                Directory.CreateDirectory(DataPath);

                DbClient = DatabaseInit.ConnectDatabase(DbPath).GetAwaiter().GetResult();
            }
            catch (System.Exception exc)
            {
                Console.WriteLine(exc.Message);
                return 1;
            }

            CreateHostBuilder(args).Build().Run();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:80", "https://*:443");
                });

        private static void MainDispose(object sender, EventArgs e)
        {
            DbClient?.Dispose();
        }
    }
}
