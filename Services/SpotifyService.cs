using Covers.Contracts;
using Covers.Contracts.Interfaces;
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
        private readonly ILogger<SpotifyService> _logger;
        private readonly SpotifyConfiguration _spotifyConfiguration;

        private SpotifyClient _spotifyClient;

        public SpotifyService(ILogger<SpotifyService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _spotifyConfiguration = configuration.GetSection(SpotifyConfiguration.Spotify).Get<SpotifyConfiguration>();
        }

        public async Task AddCallbackCodeAsync(string code)
        {
            var response = await new OAuthClient().RequestToken(
               new AuthorizationCodeTokenRequest(_spotifyConfiguration.ClientID, _spotifyConfiguration.ClientSecret, code, new Uri("https://localhost:5001/Spotify/Callback"))
            );
            var config = SpotifyClientConfig
              .CreateDefault()
              .WithAuthenticator(new AuthorizationCodeAuthenticator(_spotifyConfiguration.ClientID, _spotifyConfiguration.ClientSecret, response));

            _spotifyClient = new SpotifyClient(config);
        }

        public async Task<List<SavedAlbum>> GetAlbumsFromUserLibrary()
        {
            if (_spotifyClient == null)
            {
                return new List<SavedAlbum>();
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
                moreAlbumsAvailable -= spotifyAlbums.Count;
            }

            return spotifyAlbums;
        }
    }
}
