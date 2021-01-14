using Covers.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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

        [HttpGet("{id}"),
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

            var fileStream = new FileStream(track.Path, FileMode.Open);
            fileStream.Seek(0, SeekOrigin.Begin);
            return File(fileStream, "audio/mpeg", true);
        }
    }
}
