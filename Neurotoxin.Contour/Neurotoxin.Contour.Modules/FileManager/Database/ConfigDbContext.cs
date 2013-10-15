using System.Data.Common;
using System.Data.Entity;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    public class ConfigDbContext : DbContext
    {
        public ConfigDbContext() : base(DbConnectionFactory.GetConnection("ConfigDb"), true)
        {
            
        }

        public DbSet<FtpConnection> FtpConnections { get; set; }

    }
}