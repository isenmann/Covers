using System.Collections.Generic;

namespace Covers.Models.DTOs
{
    public class AlbumDTO
    {
        public long AlbumId { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public List<TrackDTO> Tracks { get; set; }
    }
}
