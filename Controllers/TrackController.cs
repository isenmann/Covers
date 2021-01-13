using Covers.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Covers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackController : Controller
    {
        private readonly ILogger<TrackController> _logger;
        private readonly ITrackService _trackService;

        public TrackController(ILogger<TrackController> logger, ITrackService trackService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackService = trackService ?? throw new ArgumentNullException(nameof(trackService));
        }

        [HttpGet,
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound),
         Produces("audio/mpeg"),
         ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetTrack(long id)
        {
            var track = await _trackService.GetAsync(id);
            if (track == null)
            {
                return new BadRequestObjectResult("Track not found");
            }

            var file = await System.IO.File.ReadAllBytesAsync(track.Path);

            return File(file, "audio/mpeg");
        }
    }
}
