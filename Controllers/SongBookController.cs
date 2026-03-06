using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SongBook.API.Models.Request;
using SongBook.API.Repositories;

namespace SongBook.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongBookController : ControllerBase
    {
        private readonly ISongBookRepository _repository;
        public SongBookController(ISongBookRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("songs")]
        public async Task<IActionResult> GetSongs(int? pageNo, string? searchText)
        {
            return Ok(await _repository.GetSongs());
        }

        [HttpPost]
        [Route("saveSong")]
        public async Task<IActionResult> SaveSong(Song request)
        {
            return Ok(await _repository.SaveSong(request));
        }
    }
}
