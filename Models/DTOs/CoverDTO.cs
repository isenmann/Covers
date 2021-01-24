using Covers.Persistency.Entities;

namespace Covers.Models.DTOs
{
    public class CoverDTO
    {
        public long CoverId { get; set; }
        public long AlbumId { get; set; }
        public CoverType Type { get; set; }
    }
}
