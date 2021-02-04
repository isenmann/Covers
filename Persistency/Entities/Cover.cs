using Covers.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace Covers.Persistency.Entities
{
    public class Cover
    {
        public long CoverId { get; set; }
        public CoverType Type { get; set; }
        public byte[] CoverImage { get; set; }
        public long AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }
    }
}
