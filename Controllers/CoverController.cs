using Covers.Contracts.Interfaces;
using Covers.Models.DTOs;
using Covers.Models.Requests;
using Covers.Models.Responses;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Covers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoverController : Controller
    {
        private readonly ILogger<CoverController> _logger;
        private readonly ICoverService _coverService;

        public CoverController(ILogger<CoverController> logger, ICoverService coverService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coverService = coverService ?? throw new ArgumentNullException(nameof(coverService));
        }

        [HttpGet,
         ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CoversResponse)),]
        public async Task<IActionResult> GetAsync()
        {
            var covers = await _coverService.GetAsync();

            var response = new CoversResponse
            {
                Covers = covers.Select(c => new CoverDTO
                {
                    AlbumId = c.AlbumId,
                    CoverId = c.CoverId
                }).ToList(),
                TotalCount = covers.Count()
            };
            return new OkObjectResult(response);
        }

        [HttpGet("{id}/front"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound),
         Produces("image/png"),
         ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetFrontCoverAsync(long id, [FromQuery]bool scaled)
        {
            var cover = await _coverService.GetAsync(id);
            if (cover == null)
            {
                return new BadRequestObjectResult("Front cover not found");
            }

            if (cover.FrontCover == null)
            {
                return new BadRequestObjectResult("Front cover not found");
            }

            if (scaled)
            {
                return File(ScaleCover(cover.FrontCover), "image/png");
            }

            return File(cover.FrontCover, "image/png");
        }

        [HttpGet("{id}/back"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound),
         Produces("image/png"),
         ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetBackCoverAsync(long id, [FromQuery] bool scaled)
        {
            var cover = await _coverService.GetAsync(id);
            if (cover == null)
            {
                return new BadRequestObjectResult("Back cover not found");
            }

            if (cover.BackCover == null)
            {
                return new BadRequestObjectResult("Back cover not found");
            }

            if (scaled)
            {
                return File(ScaleCover(cover.BackCover), "image/png");
            }

            return File(cover.BackCover, "image/png");
        }

        private static byte[] ScaleCover(byte[] coverImage)
        {
            using var image = new MagickImage(coverImage);
            image.Scale(new MagickGeometry { IgnoreAspectRatio = true, Width = 500, Height = 500 });
            return image.ToByteArray(MagickFormat.Png);
        }
    }
}
