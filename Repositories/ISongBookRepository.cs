using SongBook.API.Models.Request;
using SongBook.API.Models.Response;

namespace SongBook.API.Repositories
{
    public interface ISongBookRepository
    {
        Task<IEnumerable<SongResponse>> GetSongs();
        Task<long> SaveSong(Song request);
    }
}
