using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class FtpConnection : IStoredConnection
    {
        public string Name { get; set; }
        public int ConnectionImage { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

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