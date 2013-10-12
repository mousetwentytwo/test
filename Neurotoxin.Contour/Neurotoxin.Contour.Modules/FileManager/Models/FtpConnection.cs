namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    public class FtpConnection : StoredConnectionBase
    {
        public string Address;
        public int Port = 21;
        public string Username = "xbox";
        public string Password = "xbox";
    }
}