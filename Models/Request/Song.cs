namespace SongBook.API.Models.Request
{
    public class Song
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public List<string>? Stanzas { get; set; }
        public string? Category { get; set; }
        public int Year { get; set; }
        public int? StanzaNos { get; set; }
    }
}
