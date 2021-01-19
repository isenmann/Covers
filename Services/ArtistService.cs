using Covers.Contracts.Interfaces;
using Covers.Persistency;
using Covers.Persistency.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class ArtistService : IArtistService
    {
        private readonly CoversContext _context;

        public ArtistService(CoversContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Artist>> GetAsync()
        {
            return await _context.Artists.ToListAsync();
        }
    }
}
