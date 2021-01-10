using Covers.Contracts.Interfaces;
using Covers.Models.DTOs;
using Covers.Models.Requests;
using Covers.Models.Responses;
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
    public class AlbumController : Controller
    {
        private readonly ILogger<AlbumController> _logger;
        private readonly IAlbumService _albumService;

        public AlbumController(ILogger<AlbumController> logger, IAlbumService albumService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _albumService = albumService ?? throw new ArgumentNullException(nameof(albumService));
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

        [HttpPost]
        public async Task<IActionResult> CreateAlbumAsync(CreateAlbumRequest request)
        {
            await _albumService.AddAsync(request.Name);
            return new OkResult();
        }
    }
}
