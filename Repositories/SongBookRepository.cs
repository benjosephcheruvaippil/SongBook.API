using Dapper;
using SongBook.API.Data;
using SongBook.API.Models.Request;
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

        public async Task<string> GetHomePageSongs()
        {
            var query = @"
            WITH filtered_songs AS (
                SELECT *
                FROM songs
                Order by updated_at DESC
                LIMIT 5
            ),
            paged_songs AS (
                SELECT json_build_object(
                    'songId', s.song_id,
                    'title', s.title,
                    'englishTitle', s.english_title,
                    'category', s.category,
                    'stanzaNos', s.stanza_nos,
                    'stanzas', COALESCE(st.stanzas, '[]'::json)
                ) AS song_data
                FROM filtered_songs s
                LEFT JOIN LATERAL (
                    SELECT json_agg(stanza ORDER BY stanza_order) AS stanzas
                    FROM stanzas
                    WHERE song_id = s.song_id
                ) st ON true
            )
            SELECT json_build_object(
                'songs', (SELECT json_agg(song_data) FROM paged_songs)
            );";

            using var connection = _context.CreateConnection();

            return await connection.ExecuteScalarAsync<string>(query);
        }

        public async Task<string> GetSongs(int? page = 1, string? search = null)
        {
            const int pageSize = 5;
            var offset = (page - 1) * pageSize;

            var query = @"
            WITH filtered_songs AS (
                SELECT *
                FROM songs
                WHERE (@search IS NULL OR english_title ILIKE '%' || @search || '%')
            ),
            total_count AS (
                SELECT COUNT(*) AS total FROM filtered_songs
            ),
            paged_songs AS (
                SELECT json_build_object(
                    'songId', s.song_id,
                    'title', s.title,
                    'englishTitle', s.english_title,
                    'category', s.category,
                    'stanzaNos', s.stanza_nos,
                    'stanzas', COALESCE(st.stanzas, '[]'::json)
                ) AS song_data
                FROM filtered_songs s
                LEFT JOIN LATERAL (
                    SELECT json_agg(stanza ORDER BY stanza_order) AS stanzas
                    FROM stanzas
                    WHERE song_id = s.song_id
                ) st ON true
                ORDER BY s.song_id
                LIMIT @pageSize OFFSET @offset
            )
            SELECT json_build_object(
                'totalPages', CEIL((SELECT total FROM total_count)::decimal / @pageSize),
                'songs', (SELECT json_agg(song_data) FROM paged_songs)
            );";

            using var connection = _context.CreateConnection();

            return await connection.ExecuteScalarAsync<string>(query, new
            {
                search,
                pageSize,
                offset
            });
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

        public async Task<bool> DeleteSong(long? songId)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var query = @"DELETE FROM songs WHERE song_id = @SongId";
                await connection.ExecuteAsync(query, new { SongId = songId });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
