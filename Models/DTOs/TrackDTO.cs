namespace Covers.Models.DTOs
{
    public class TrackDTO
    {
        public long TrackId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string SpotifyUri { get; set; }
    }
}
