namespace Covers.Models.DTOs
{
    public class AlbumOverviewDTO
    {
        public long FrontCoverId { get; set; }
        public long BackCoverId { get; set; }
        public long AlbumId { get; set; }
        public string AlbumName { get; set; }
        public string ArtistName { get; set; }
    }
}
