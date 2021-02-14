using Covers.Contracts;
using Covers.Contracts.Interfaces;
using Covers.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class SpotifyService : ISpotifyService
    {
        public string AccessToken { get; private set; }

        private readonly ILogger<SpotifyService> _logger;
        private readonly IHubContext<CoversHub> _hubContext;
        private readonly SpotifyConfiguration _spotifyConfiguration;
        private SpotifyClient _spotifyClient;

        public SpotifyService(ILogger<SpotifyService> logger, IConfiguration configuration, IHubContext<CoversHub> hubContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _spotifyConfiguration = configuration.GetSection(SpotifyConfiguration.Spotify).Get<SpotifyConfiguration>();
        }

        public async Task AddCallbackCodeAsync(string code)
        {
            var response = await new OAuthClient().RequestToken(
               new AuthorizationCodeTokenRequest(_spotifyConfiguration.ClientID, _spotifyConfiguration.ClientSecret, code, new Uri("https://localhost:5001/Spotify/Callback"))
            );

            AccessToken = response.AccessToken;
            await _hubContext.Clients.All.SendAsync("SpotifyTokenRefresh", AccessToken);

            var config = SpotifyClientConfig
              .CreateDefault()
              .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifyConfiguration.ClientID, _spotifyConfiguration.ClientSecret, response));

            _spotifyClient = new SpotifyClient(config);
        }

        public async Task<List<SavedAlbum>> GetAlbumsFromUserLibrary()
        {
            if (_spotifyClient == null)
            {
                return null;
            }

            var spotifyAlbums = new List<SavedAlbum>();

            int limit = 50;
            int offset = 0;

            var spotifyAlbumsPaged = await _spotifyClient.Library.GetAlbums(new LibraryAlbumsRequest { Limit = limit, Offset = offset });
            spotifyAlbums.AddRange(spotifyAlbumsPaged.Items);

            int moreAlbumsAvailable = spotifyAlbumsPaged.Total.Value - spotifyAlbums.Count;

            while (moreAlbumsAvailable > 0)
            {
                spotifyAlbumsPaged = await _spotifyClient.Library.GetAlbums(new LibraryAlbumsRequest { Limit = limit, Offset = spotifyAlbums.Count });
                spotifyAlbums.AddRange(spotifyAlbumsPaged.Items);
                moreAlbumsAvailable -= limit;
            }

            return spotifyAlbums;
        }

        public async Task Play(string spotifyUri, string deviceId)
        {
            if (_spotifyClient == null)
            {
                return;
            }

            await _spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest { DeviceId = deviceId, Uris = new List<string> { spotifyUri } });
        }

        public async Task Resume(string deviceId)
        {
            if (_spotifyClient == null)
            {
                return;
            }

            await _spotifyClient.Player.ResumePlayback(new PlayerResumePlaybackRequest { DeviceId = deviceId });
        }

        public async Task Pause(string deviceId)
        {
            if (_spotifyClient == null)
            {
                return;
            }

            await _spotifyClient.Player.PausePlayback(new PlayerPausePlaybackRequest { DeviceId = deviceId });
        }

        public async Task SeekStepTo(string deviceId, long offset)
        {
            if (_spotifyClient == null)
            {
                return;
            }

            var currentPlayback = await _spotifyClient.Player.GetCurrentPlayback();
            await _spotifyClient.Player.SeekTo(new PlayerSeekToRequest(currentPlayback.ProgressMs + offset) { DeviceId = deviceId });
        }

        public async Task SetVolume(string deviceId, double volume)
        {
            if (_spotifyClient == null)
            {
                return;
            }

            int vol = Convert.ToInt32(100 * volume);
            await _spotifyClient.Player.SetVolume(new PlayerVolumeRequest(vol) { DeviceId = deviceId });
        }

        public async Task<CurrentlyPlayingContext> RequestPlaybackState(string deviceId)
        {
            if (_spotifyClient == null)
            {
                return null;
            }

            return await _spotifyClient.Player.GetCurrentPlayback();
        }
    }
}
