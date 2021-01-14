using Covers.Models.DTOs;
using System.Collections.Generic;

namespace Covers.Models.Responses
{
    public class AlbumOverviewResponse
    {
        public int TotalCount { get; set; }
        public List<CoverDTO> Albums { get; set; }
    }
}
