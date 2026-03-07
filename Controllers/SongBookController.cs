using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SongBook.API.Models.Request;
using SongBook.API.Repositories;
using System.Text.Json;

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
            var json = await _repository.GetSongs(pageNo, searchText);
            return Ok(JsonSerializer.Deserialize<object>(json));
            //return Content(json, "application/json");
        }

        [HttpPost]
        [Route("saveSong")]
        public async Task<IActionResult> SaveSong(Song request)
        {
            return Ok(await _repository.SaveSong(request));
        }
    }
}
