using Covers.Persistency.Entities;
using Microsoft.EntityFrameworkCore;

namespace Covers.Persistency
{
    public class CoversContext : DbContext
    {
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Cover> Covers { get; set; }

        public CoversContext(DbContextOptions<CoversContext> options) : base(options)
        {

        }
    }
}
