using System.ComponentModel.DataAnnotations;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    [Table("FtpConnection")]
    public class FtpConnection : IStoredConnection
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("ConnectionImage")]
        public int ConnectionImage { get; set; }

        [Column("Address")]
        public string Address { get; set; }

        [Column("Port")]
        public int Port { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        [Column("Password")]
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