using Covers.Persistency.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Contracts.Interfaces
{
    public interface ICoverService
    {
        Task<IEnumerable<Cover>> GetAsync();
    }
}
