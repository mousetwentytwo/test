using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    public class FtpConnection : StoredConnectionBase
    {
        public string Address;
        public int Port = 21;
        public string Username = "xbox";
        public string Password = "xbox";

        public FtpConnection()
        {
            ConnectionImage = ConnectionImage.Fat;
        }

        public FtpConnection Clone()
        {
            return new FtpConnection
                       {
                           Name = Name,
                           ConnectionImage = ConnectionImage,
                           Address = Address,
                           Port = Port,
                           Username = Username,
                           Password = Password
                       };
        }
    }
}