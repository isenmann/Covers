using Covers.Contracts.Interfaces;
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
                Covers = covers.Select(c => new Models.DTOs.CoverDTO
                {
                    AlbumId = c.AlbumId,
                    CoverId = c.CoverId,
                    CoverImage = string.Empty
                }).ToList(),
                TotalCount = covers.Count()
            };
            return new OkObjectResult(response);
        }
    }
}
