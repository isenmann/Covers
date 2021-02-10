using Covers.Contracts;
using Covers.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Covers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyController : Controller
    {
        private readonly SpotifyConfiguration _spotifyConfiguration;
        private readonly ILogger<SpotifyController> _logger;
        private readonly ISpotifyService _spotifyService;

        public SpotifyController(ILogger<SpotifyController> logger, ISpotifyService spotifyService, IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _spotifyConfiguration = configuration.GetSection(SpotifyConfiguration.Spotify).Get<SpotifyConfiguration>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _spotifyService = spotifyService ?? throw new ArgumentNullException(nameof(spotifyService));
        }

        [HttpGet("Login"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login()
        {
            if (string.IsNullOrWhiteSpace(_spotifyConfiguration.ClientID) ||
                string.IsNullOrWhiteSpace(_spotifyConfiguration.ClientSecret))
            {
                return Ok();
            }

            var loginRequest = new LoginRequest(
              new Uri("https://localhost:5001/Spotify/Callback"),
              _spotifyConfiguration.ClientID,
              LoginRequest.ResponseType.Code)
            {
                Scope = new[] { Scopes.UserLibraryRead }
            };
            var uri = loginRequest.ToUri();
            return Redirect(uri.AbsoluteUri);
        }

        [HttpGet("Callback"),
         ProducesResponseType(StatusCodes.Status200OK),
         ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CallbackAsync(string code)
        {
            await _spotifyService.AddCallbackCodeAsync(code);
            return Ok();
        }
    }
}
