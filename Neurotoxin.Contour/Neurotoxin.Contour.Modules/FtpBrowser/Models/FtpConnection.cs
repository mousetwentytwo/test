using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    public class FtpConnection : StoredConnectionBase
    {
        public string Address;
        public int Port;
        public string Username;
        public string Password;
    }
}