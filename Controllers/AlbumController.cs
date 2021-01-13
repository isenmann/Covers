﻿using Covers.Contracts.Interfaces;
using Covers.Models.DTOs;
using Covers.Models.Requests;
using Covers.Models.Responses;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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

        [HttpGet("Covers"),
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

        [HttpGet("Cover"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status404NotFound),
         Produces("image/png"),
         ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetCoverAsync([FromQuery]CoverRequest request)
        {
            var cover = await _coverService.GetAsync(request.CoverId);
            if (cover == null)
            {
                return new BadRequestObjectResult("Cover not found");
            }

            if (request.Scaled)
            {
                return File(ScaleCover(cover.FrontCover), "image/png");
            }

            return File(cover.FrontCover, "image/png");
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
                }).ToList()
            };
            return new OkObjectResult(response);
        }

        [HttpPost("Cover"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status400BadRequest),
         RequestSizeLimit(5242880)]
        public async Task<IActionResult> AddAlbumCoverAsync([FromForm]AddAlbumCoverRequest request)
        {
            var album = await _albumService.GetAsync(request.AlbumId);
            if (album == null)
            {
                return new BadRequestObjectResult("Album not found");
            }

            using var binaryReader = new BinaryReader(request.FrontCover.OpenReadStream());
            var imageBytes = binaryReader.ReadBytes((int)request.FrontCover.Length);
            using var image = new MagickImage(imageBytes);

            if (album.Cover == null)
            {
                album.Cover = new Persistency.Entities.Cover();
            }
            
            album.Cover.FrontCover = image.ToByteArray(MagickFormat.Png);

            await _albumService.UpdateAsync(album);

            return new OkResult();
        }

        private static byte[] ScaleCover(byte[] coverImage)
        {
            using var image = new MagickImage(coverImage);
            image.Scale(new MagickGeometry { IgnoreAspectRatio = true, Width = 250, Height = 250 });
            return image.ToByteArray(MagickFormat.Png);
        }
    }
}
