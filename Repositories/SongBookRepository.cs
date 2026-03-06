using Dapper;
using SongBook.API.Data;
using SongBook.API.Models.Request;
using SongBook.API.Models.Response;
using System;
using System.Data;

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

        public async Task<long> SaveSong(Song request)
        {
            try
            {
                using var connection = _context.CreateConnection();
                connection.Open();
                using var transaction = connection.BeginTransaction();

                long songId;

                if (request.SongId != null && request.SongId != 0)
                {
                    var updateQuery = @"
            UPDATE songs
            SET 
                title = @Title,
                english_title = @EnglishTitle,
                category = @Category,
                stanza_nos = @StanzaNos,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE song_id = @SongId
            RETURNING song_id;";

                    request.UpdatedAt = DateTime.UtcNow;

                    songId = await connection.ExecuteScalarAsync<long>(
                        updateQuery, request, transaction);
                }
                else
                {
                    var insertQuery = @"
            INSERT INTO songs
            (title, english_title, category, stanza_nos, created_at, created_by)
            VALUES
            (@Title, @EnglishTitle, @Category, @StanzaNos, @CreatedAt, @CreatedBy)
            RETURNING song_id;";

                    request.CreatedAt = DateTime.UtcNow;

                    songId = await connection.ExecuteScalarAsync<long>(
                        insertQuery, request, transaction);
                }

                if (request.Stanzas != null)
                {
                    await DeleteStanzaIfExists(songId, connection, transaction);

                    int order = 1;

                    foreach (var stanza in request.Stanzas)
                    {
                        var insertStanza = @"
                INSERT INTO stanzas
                (song_id, stanza, stanza_order, created_at, created_by)
                VALUES
                (@SongId, @Stanza, @StanzaOrder, @CreatedAt, @CreatedBy);";

                        await connection.ExecuteAsync(insertStanza, new
                        {
                            SongId = songId,
                            Stanza = stanza,
                            StanzaOrder = order++,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = request.CreatedBy
                        }, transaction);
                    }
                }

                transaction.Commit();

                return songId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving song: {ex.Message}");
            }
        }

        public async Task DeleteStanzaIfExists(long songId, IDbConnection connection, IDbTransaction transaction)
        {
            var query = @"DELETE FROM stanzas WHERE song_id = @SongId";

            await connection.ExecuteAsync(query, new { SongId = songId }, transaction);
        }
    }
}
