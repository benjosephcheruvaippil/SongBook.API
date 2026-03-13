using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SongBook.API.Models.Request;
using SongBook.API.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SongBook.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongBookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISongBookRepository _repository;
        public SongBookController(ISongBookRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> LoginAsync(string? userName, string? password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return BadRequest("Username and password required");

            if (userName == "ben" && password == "ben")
            {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                var signingCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512
                );

                var subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Email, userName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                });

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    Expires = DateTime.UtcNow.AddMinutes(2),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new
                {
                    token = tokenHandler.WriteToken(token)
                });
            }

            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("homPageSongs")]
        public async Task<IActionResult> GetHomPageSongs()
        {
            var json = await _repository.GetHomePageSongs();
            return Ok(JsonSerializer.Deserialize<object>(json));
        }

        [Authorize]
        [HttpGet]
        [Route("songs")]
        public async Task<IActionResult> GetSongs(int? pageNo, string? searchText)
        {
            var json = await _repository.GetSongs(pageNo, searchText);
            return Ok(JsonSerializer.Deserialize<object>(json));
            //return Content(json, "application/json");
        }

        [Authorize]
        [HttpPost]
        [Route("saveSong")]
        public async Task<IActionResult> SaveSong(Song request)
        {
            return Ok(await _repository.SaveSong(request));
        }

        [Authorize]
        [HttpDelete]
        [Route("deleteSong")]
        public async Task<IActionResult> DeleteSong(long? songId)
        {
            return Ok(await _repository.DeleteSong(songId));
        }
    }
}
