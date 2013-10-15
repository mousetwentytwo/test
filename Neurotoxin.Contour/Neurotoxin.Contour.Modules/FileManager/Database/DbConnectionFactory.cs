using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    public static class DbConnectionFactory
    {
        public static DbConnection GetConnection(string name)
        {
            var connection = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0").CreateConnection();
            connection.ConnectionString = string.Format(@"Data Source=|DataDirectory|\{0}.sdf;Persist Security Info=False;", name);
            return connection;
        }
    }
}
