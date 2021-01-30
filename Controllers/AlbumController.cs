using Covers.Contracts.Interfaces;
using Covers.Models.DTOs;
using Covers.Models.Requests;
using Covers.Models.Responses;
using Covers.Persistency.Entities;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Covers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlbumController : Controller
    {
        private readonly ILogger<AlbumController> _logger;
        private readonly IAlbumService _albumService;
        private readonly ICoverService _coverService;

        public AlbumController(ILogger<AlbumController> logger, IAlbumService albumService, ICoverService coverService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _albumService = albumService ?? throw new ArgumentNullException(nameof(albumService));
            _coverService = coverService ?? throw new ArgumentNullException(nameof(coverService));
        }

        [HttpGet("{id}"),
         ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlbumDTO)),
         ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAsync(long id)
        {
            var album = await _albumService.GetAsync(id);
            if (album == null)
            {
                return new BadRequestObjectResult("Album not found");
            }

            var artistUnique = album.Tracks.Select(t => t.ArtistId).Distinct().Count() == 1;

            var response = new AlbumDTO
            {
                AlbumId = album.AlbumId,
                Name = album.Name,
                Artist = artistUnique ? album.Tracks.First().Artist.Name : "Various Artists",
                Tracks = album.Tracks.Select(t => new TrackDTO
                {
                    TrackId = t.TrackId,
                    Artist = t.Artist.Name,
                    Name = t.Name,
                    Number = t.Number
                }).OrderBy(t => t.Number).ToList()
            };
            return new OkObjectResult(response);
        }

        [HttpGet("Overview"),
         ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlbumOverviewResponse)),
         ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOverviewAsync()
        {
            var albums = await _albumService.GetAsync();

            var response = new AlbumOverviewResponse
            {
                Albums = albums.OrderBy(a=> a.Name).Select(a => new AlbumOverviewDTO
                {
                    AlbumId = a.AlbumId,
                    FrontCoverId = a.Covers.FirstOrDefault(c => c.Type == CoverType.Front)?.CoverId ?? -1,
                    BackCoverId = a.Covers.FirstOrDefault(c => c.Type == CoverType.Back)?.CoverId ?? -1,
                    AlbumName = a.Name,
                    ArtistName = a.Artist != null ? a.Artist.Name : "Various Artists"
                }).ToList(),
                TotalCount = albums.Count
            };

            return new OkObjectResult(response);
        }

        [HttpPost("FrontCover"),
         ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CoverAddedResponse)),
         ProducesResponseType(StatusCodes.Status400BadRequest),
         RequestSizeLimit(5242880)]
        public async Task<IActionResult> AddAlbumFrontCoverAsync([FromForm]AddAlbumCoverRequest request)
        {
            return await UpdateAlbumCover(request.AlbumId, request.Cover, true);
        }

        [HttpPost("BackCover"),
         ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CoverAddedResponse)),
         ProducesResponseType(StatusCodes.Status400BadRequest),
         RequestSizeLimit(5242880)]
        public async Task<IActionResult> AddAlbumBackCoverAsync([FromForm] AddAlbumCoverRequest request)
        {
            return await UpdateAlbumCover(request.AlbumId, request.Cover, false);
        }

        private async Task<IActionResult> UpdateAlbumCover(long albumId, IFormFile cover, bool front)
        {
            var album = await _albumService.GetAsync(albumId);
            if (album == null)
            {
                return new BadRequestObjectResult("Album not found");
            }

            using var binaryReader = new BinaryReader(cover.OpenReadStream());
            var imageBytes = binaryReader.ReadBytes((int)cover.Length);
            using var image = new MagickImage(imageBytes);
            if (image.Width > 800)
            {
                image.Scale(new MagickGeometry { IgnoreAspectRatio = false, Width = 800 });
            }

            if (album.Covers == null)
            {
                album.Covers = new List<Cover>();
            }

            if (front)
            {
                var frontCover = album.Covers.FirstOrDefault(c => c.Type == CoverType.Front);
                if (frontCover != null)
                {
                    await _coverService.DeleteCoverAsync(frontCover);
                }

                frontCover = new Cover
                {
                    AlbumId = albumId,
                    Type = CoverType.Front,
                    CoverImage = image.ToByteArray(MagickFormat.Png)
                };
                album.Covers.Add(frontCover);
            }
            else
            {
                var backCover = album.Covers.FirstOrDefault(c => c.Type == CoverType.Back);
                if (backCover != null)
                {
                    await _coverService.DeleteCoverAsync(backCover);
                }

                backCover = new Cover
                {
                    AlbumId = albumId,
                    Type = CoverType.Back,
                    CoverImage = image.ToByteArray(MagickFormat.Png)
                };
                album.Covers.Add(backCover);
            }

            await _albumService.UpdateAsync(album);
            long? coverId = -1;
            if (front)
            {
                coverId = album.Covers.FirstOrDefault(c => c.Type == CoverType.Front)?.CoverId;
            }
            else
            {
                coverId = album.Covers.FirstOrDefault(c => c.Type == CoverType.Back)?.CoverId;
            }

            var response = new CoverAddedResponse
            {
                CoverId = coverId ?? -1
            };

            return new OkObjectResult(response);
        }
    }
}
