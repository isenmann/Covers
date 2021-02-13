using Covers.Persistency.Entities;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ISpotifyService
    {
        public string AccessToken { get; }
        Task AddCallbackCodeAsync(string code);
        Task<List<SavedAlbum>> GetAlbumsFromUserLibrary();
        Task Play(string spotifyUri, string deviceId);
        Task Pause(string deviceId);
        Task Resume(string deviceId);
        Task SeekStepTo(string deviceId, long offset);
    }
}