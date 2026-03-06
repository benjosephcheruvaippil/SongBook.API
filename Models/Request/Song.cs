namespace SongBook.API.Models.Request
{
    public class Song
    {
        public long? SongId { get; set; }
        public string? Title { get; set; }
        public string? EnglishTitle { get; set; }
        public string? Artist { get; set; }
        public List<string>? Stanzas { get; set; }
        public string? Category { get; set; }
        public int Year { get; set; }
        public int? StanzaNos { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
