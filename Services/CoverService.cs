using Covers.Contracts.Interfaces;
using Covers.Persistency;
using Covers.Persistency.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class CoverService : ICoverService
    {
        private readonly CoversContext _context;

        public CoverService(CoversContext context)
        {
            _context = context;
        }
        public async Task<List<Cover>> GetAsync()
        {
            return await _context.Covers.AsNoTracking().ToListAsync();
        }

        public async Task<List<Tuple<long, long>>> GetCoverAndAlbumIdAsync()
        {
            return await _context.Covers.AsNoTracking().Select(c => new Tuple<long, long>(c.CoverId, c.AlbumId)).ToListAsync();
        }

        public async Task<Cover> GetAsync(long id)
        {
            return await _context.Covers.FindAsync(id);
        }

        public async Task<List<Cover>> GetPagedAsync(int pageNumber)
        {
            return await _context.Covers.AsNoTracking().Skip((pageNumber - 1) * 40).Take(40).ToListAsync();
        }

        public async Task DeleteCoverAsync(Cover cover)
        {
            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoverAsync(long id)
        {
            var cover = await _context.Covers.FindAsync(id);
            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync();
        }
    }
}
