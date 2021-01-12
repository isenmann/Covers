using Microsoft.AspNetCore.Http;

namespace Covers.Models.Requests
{
    public class AddAlbumCoverRequest
    {
        public long AlbumId { get; set; }
        public IFormFile FrontCover { get; set; }
        public IFormFile BackCover { get; set; }
    }
}
