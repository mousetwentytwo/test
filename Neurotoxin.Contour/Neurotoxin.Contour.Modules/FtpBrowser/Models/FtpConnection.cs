namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    public class FtpConnection : StoredConnectionBase
    {
        public string Address;
        public int Port = 21;
        public string Username;
        public string Password;
    }
}