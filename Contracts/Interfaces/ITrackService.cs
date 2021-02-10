using Covers.Persistency.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ITrackService
    {
        Task<List<Track>> GetAsync();
        Task<Track> GetAsync(long id);
    }
}
