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
        public string DefaultPath { get; set; }
        public bool UsePassiveMode { get; set; }
        public string HttpUsername { get; set; }
        public string HttpPassword { get; set; }

        public FtpConnection Clone()
        {
            return new FtpConnection
                       {
                           Name = Name,
                           ConnectionImage = ConnectionImage,
                           Address = Address,
                           Port = Port,
                           Username = Username,
                           Password = Password,
                           DefaultPath = DefaultPath,
                           UsePassiveMode = UsePassiveMode,
                           HttpUsername = HttpUsername,
                           HttpPassword = HttpPassword
                       };
        }
    }
}