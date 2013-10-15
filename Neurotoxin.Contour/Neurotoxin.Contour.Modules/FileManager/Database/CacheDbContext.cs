using System.Data.Entity;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    public class CacheDbContext : DbContext
    {
        public CacheDbContext() : base(DbConnectionFactory.GetConnection("CacheDb"), true)
        {
            
        }

        public DbSet<CacheEntry> CacheEntries { get; set; }

    }
}