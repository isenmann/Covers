using System.ComponentModel.DataAnnotations.Schema;

namespace Covers.Persistency.Entities
{
    public class Track
    {
        public long TrackId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string SpotifyId { get; set; }
        public string SpotifyUri { get; set; }
        public long ArtistId { get; set; }
        [ForeignKey("ArtistId")]
        public virtual Artist Artist { get; set; }
        public long AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }
    }
}
