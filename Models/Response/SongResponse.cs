namespace SongBook.API.Models.Response
{
    public class SongResponse
    {
        public long SongId { get; set; }
        public string? Title { get; set; }
        public string? EnglishTitle { get; set; }
        public string? Artist { get; set; }
        public List<string>? Stanzas { get; set; }
        public string? Category { get; set; }
        public int Year { get; set; }
        public int? StanzaNos { get; set; }
    }
}
