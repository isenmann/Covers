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

        public async Task AddAsync(string name)
        {
            var album = new Album
            {
                Name = name,
                Path = "some path",
                Cover = new Cover
                {
                    Path = "some path"
                },
                Tracks = new List<Track>
                {
                    new Track
                    {
                        Name = "track name",
                        Number = 1,
                        Path = "sopm pat",
                        Artist = new Artist
                        {
                            Name = "some artist"
                        }
                    }
                }
            };

            await _context.Albums.AddAsync(album);
            await _context.SaveChangesAsync();
        }
    }
}
