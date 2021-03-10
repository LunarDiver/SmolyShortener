using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DbColumn = SmolyShortener.Database.DbColumn;
using System.Collections.Generic;

namespace SmolyShortener.Controllers
{
    [ApiController]
    public class ShorteningController : ControllerBase
    {
        private const string _dbTable = "shorten";

        private DbColumn[] _dbColumnInfo
        {
            get
            {
                return Database.DatabaseInit.Tables[_dbTable];
            }
        }

        [HttpGet("s/{id}")]
        public async Task<IActionResult> GetHttp(string id)
        {
            string redirect = await GetRedirect(id);

            if (!await EntryExists(id) || string.IsNullOrWhiteSpace(redirect))
                return Redirect("/");

            return Redirect(await GetRedirect(id));
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> PostHttp()
        {
            string id = await CreateUniqueId();

            using var reader = new StreamReader(Request.Body);
            string url = await reader.ReadToEndAsync();

            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri uriUrl)
                && !uriUrl.IsLoopback
                && (uriUrl.Scheme == Uri.UriSchemeHttp || uriUrl.Scheme == Uri.UriSchemeHttps);

            if (!isValidUrl)
                return BadRequest($"Invalid URL: {url}");

            await WriteEntry(id, url);

            return Ok($"{Request.Scheme}://{Request.Host}/s/{id}");
        }

        private async Task<string> CreateUniqueId()
        {
            int maxPossibilites = (int)Math.Round(Math.Pow(byte.MaxValue, 2));

            var rng = new Random();
            byte[] random = new byte[2];
            string id = string.Empty;

            int count;
            for (count = 0; count < maxPossibilites; count++)
            {
                rng.NextBytes(random);
                id = Convert.ToBase64String(random, Base64FormattingOptions.None)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');

                if (!await EntryExists(id))
                    break;
            }
            if (count >= maxPossibilites)
                throw new ArithmeticException("Could not find id that is not yet in use.");

            return id;
        }

        private async Task WriteEntry(string id, string url)
        {
            await Program.DbClient.WriteAsync(
                _dbTable,
                ("id", id),
                ("redirect", url),
                ("expiry", (DateTime.UtcNow + TimeSpan.FromMinutes(5)).ToString("u")));
        }

        private async Task<bool> EntryExists(string id)
        {
            string primary = _dbColumnInfo.FirstOrDefault(a => a.IsPrimary).Name;

            IAsyncEnumerable<object[]> res = Program.DbClient.ReadAsync(
                _dbTable,
                new[]
                {
                    primary
                },
                1,
                new[]
                {
                    $"{primary} = '{id}'"
                });

            return (await res.FirstOrDefaultAsync()) != null;
        }

        private async Task<string> GetRedirect(string id)
        {
            return (await Program.DbClient
                .ReadAsync(
                    _dbTable,
                    new[] { "redirect" },
                    1,
                    new[]
                    {
                        $"id = '{id}'",
                        $"datetime(expiry) > datetime('now')"
                    })
                .FirstOrDefaultAsync())?[0]?
                .ToString();
        }
    }
}