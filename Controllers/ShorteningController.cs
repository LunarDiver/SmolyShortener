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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHttp(string id)
        {
            if (!await EntryExists(id))
                return Redirect("/");

            return Ok($"Test ok: {id}");
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
    }
}