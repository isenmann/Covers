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
        public async Task<IEnumerable<Cover>> GetAsync()
        {
            return await _context.Covers.AsNoTracking().ToListAsync();
        }

        public async Task<Cover> GetAsync(long id)
        {
            return await _context.Covers.FindAsync(id);
        }

        public async Task<IEnumerable<Cover>> GetPagedAsync(int pageNumber)
        {
            return await _context.Covers.AsNoTracking().Skip((pageNumber - 1) * 40).Take(40).ToListAsync();
        }
    }
}
