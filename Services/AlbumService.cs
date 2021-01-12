using Covers.Contracts.Interfaces;
using Covers.Persistency;
using Covers.Persistency.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Covers.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly CoversContext _context;

        public AlbumService(CoversContext context)
        {
            _context = context;
        }

        public async Task<Album> GetAsync(long id)
        {
            return await _context.Albums.FirstOrDefaultAsync(a => a.AlbumId == id);
        }

        public async Task<List<Album>> GetAsync()
        {
            return await _context.Albums.ToListAsync();
        }

        public async Task AddAsync(Album album)
        {
            await _context.Albums.AddAsync(album);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(List<Album> albums)
        {
            await _context.Albums.AddRangeAsync(albums);
            await _context.SaveChangesAsync();
        }
    }
}
