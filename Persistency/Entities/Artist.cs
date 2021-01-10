using System.Collections.Generic;

namespace Covers.Persistency.Entities
{
    public class Artist
    {
        public long ArtistId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Track> Tracks { get; set; }
    }
}
