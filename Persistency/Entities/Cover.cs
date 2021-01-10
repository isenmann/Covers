using System.ComponentModel.DataAnnotations.Schema;

namespace Covers.Persistency.Entities
{
    public class Cover
    {
        public long CoverId { get; set; }
        public string Path { get; set; }
        public long AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }
    }
}
