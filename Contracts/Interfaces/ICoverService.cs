using Covers.Persistency.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ICoverService
    {
        Task<List<Cover>> GetAsync();
        Task<Cover> GetAsync(long id);
        Task<List<Tuple<long, long>>> GetCoverAndAlbumIdAsync();
        Task DeleteCoverAsync(Cover cover);
        Task DeleteCoverAsync(long id);
    }
}
