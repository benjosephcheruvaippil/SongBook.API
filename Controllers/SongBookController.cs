using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SongBook.API.Models.Request;

namespace SongBook.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongBookController : ControllerBase
    {
        public SongBookController()
        {
        }

        [HttpGet]
        [Route("songs")]
        public async Task<IActionResult> GetSongs(int pageNo, string searchText)
        {
            return Ok("Hello World");
        }

        [HttpPost]
        [Route("saveSong")]
        public async Task<IActionResult> SaveSong(Song request)
        {
            return Ok($"Hello {request.ToString()}");
        }

    }
}
