using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmolyShortener.Controllers
{
    [ApiController]
    public class ShorteningController : ControllerBase
    {
        // var client = new DatabaseClient("test.db");
        // await client.CreateTable("test", "testcol1 INT PRIMARY KEY NOT NULL", "testcol2 TEXT");
        // await client.Write("test", new[]
        // {
        //     "'testcol1'",
        //     "'testcol2'"
        // }, new[]
        // {
        //     69.ToString(),
        //     "testvalue"
        // });
        // client.Dispose();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHttp(string id)
        {
            return Ok($"Test ok: {id}");
        }

        private bool EntryExists(string id)
        {
            throw new NotImplementedException();
        }
    }
}