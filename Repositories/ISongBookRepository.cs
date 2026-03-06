using SongBook.API.Models.Response;

namespace SongBook.API.Repositories
{
    public interface ISongBookRepository
    {
        Task<IEnumerable<SongResponse>> GetSongs();
    }
}
