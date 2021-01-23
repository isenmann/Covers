using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Covers.Persistency.Entities
{
    public class Album
    {
        public long AlbumId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public virtual Cover Cover { get; set; }
        public virtual Artist Artist { get; set; }
        public virtual ICollection<Track> Tracks { get; set; }
    }
}
