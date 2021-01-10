using Covers.Persistency.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface IAlbumService
    {
        Task<Album> GetAsync(long id);
        Task AddAsync(string name);
    }
}
