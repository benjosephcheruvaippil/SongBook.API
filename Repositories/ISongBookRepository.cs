using SongBook.API.Models.Request;
using SongBook.API.Models.Response;

namespace SongBook.API.Repositories
{
    public interface ISongBookRepository
    {
        Task<string> GetSongs(int? page = 1, string? search = null);
        Task<long> SaveSong(Song request);
        Task<bool> DeleteSong(long? songId);
    }
}
