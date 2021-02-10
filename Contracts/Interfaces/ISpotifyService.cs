using Covers.Persistency.Entities;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ISpotifyService
    {
        Task AddCallbackCodeAsync(string code);
        Task<List<SavedAlbum>> GetAlbumsFromUserLibrary();
    }
}