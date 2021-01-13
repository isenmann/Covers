using Microsoft.AspNetCore.Mvc;

namespace Covers.Models.Requests
{
    [BindProperties]
    public class CoverRequest
    {
        public long CoverId { get; set; }
        public bool Scaled { get; set; }
    }
}
