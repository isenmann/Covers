using Covers.Contracts.Interfaces;
using Covers.Persistency;
using Covers.Persistency.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class TrackService : ITrackService
    {
        private readonly CoversContext _context;

        public TrackService(CoversContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Track>> GetAsync()
        {
            return await _context.Tracks.AsNoTracking().ToListAsync();
        }

        public async Task<Track> GetAsync(long id)
        {
            return await _context.Tracks.FindAsync(id);
        }
    }
}
