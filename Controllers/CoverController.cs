using Covers.Contracts.Interfaces;
using Covers.Models.DTOs;
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
                    CoverId = c.CoverId,
                    Type = c.Type
                }).ToList(),
                TotalCount = covers.Count
            };
            return new OkObjectResult(response);
        }

        [HttpGet("{id}"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound),
         Produces("image/png")
         /*ResponseCache(Duration = 86400)*/]
        public async Task<IActionResult> GetCoverAsync(long id, [FromQuery] int? size)
        {
            var cover = await _coverService.GetAsync(id);
            if (cover == null)
            {
                return new BadRequestObjectResult("Cover not found");
            }

            if (cover == null)
            {
                return new BadRequestObjectResult("Cover not found");
            }

            if (size.HasValue)
            {
                return File(ScaleCover(cover.CoverImage, size.Value), "image/png");
            }

            return File(cover.CoverImage, "image/png");
        }

        private static byte[] ScaleCover(byte[] coverImage, int size = 500)
        {
            using var image = new MagickImage(coverImage);
            image.Scale(new MagickGeometry { IgnoreAspectRatio = true, Width = size, Height = size });
            return image.ToByteArray(MagickFormat.Png);
        }
    }
}
