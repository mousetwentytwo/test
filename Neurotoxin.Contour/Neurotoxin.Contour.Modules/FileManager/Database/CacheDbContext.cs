using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    public class CacheDbContext : DbContext
    {
        public CacheDbContext(DbConnection connection) : base(connection, true)
        {
            
        }

        public DbSet<CacheEntry> CacheEntries { get; set; }

    }
}