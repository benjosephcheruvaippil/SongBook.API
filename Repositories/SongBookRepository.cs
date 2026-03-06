using Dapper;
using SongBook.API.Data;
using SongBook.API.Models.Response;
using System;

namespace SongBook.API.Repositories
{
    public class SongBookRepository : ISongBookRepository
    {
        private readonly DapperContext _context;
        public SongBookRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SongResponse>> GetSongs()
        {
            try
            {
                var query = "SELECT title as Title,english_title as EnglishTitle, category as Category, stanza_nos as StanzaNos FROM songs";

                using var connection = _context.CreateConnection();
                var result = await connection.QueryAsync<SongResponse>(query);
                return result;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error fetching songs: {ex.Message}");
            }
        }
    }
}
