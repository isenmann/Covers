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
        Task<List<Cover>> GetPagedAsync(int pageNumber);
        Task<List<Tuple<long, long>>> GetCoverAndAlbumIdAsync();
    }
}
