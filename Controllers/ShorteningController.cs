using System;
using System.Security.AccessControl;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace SmolyShortener.Controllers
{
    [ApiController]
    public class ShorteningController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetHttp(string id)
        {
            var client = new DatabaseClient("test.db");
            string res = client.TestConnection().GetAwaiter().GetResult();
            client.Dispose();
            return Ok($"Test ok: {id}, {res}");
        }

        private bool EntryExists(string id)
        {
            throw new NotImplementedException();
        }
    }
}