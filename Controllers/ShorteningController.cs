using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmolyShortener.Controllers
{
    [ApiController]
    public class ShorteningController : ControllerBase
    {
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