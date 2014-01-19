using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpConnectionItemViewModel : ViewModelBase, IStoredConnectionViewModel
    {
        public FtpConnection Model { get; private set; }

        private const string NAME = "Name";
        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value ?? string.Empty; NotifyPropertyChanged(NAME); }
        }

        private const string CONNECTIONIMAGE = "ConnectionImage";
        public ConnectionImage ConnectionImage
        {
            get { return (ConnectionImage)Model.ConnectionImage; }
            set
            {
                Model.ConnectionImage = (int)value;
                _thumbnail = null;
                NotifyPropertyChanged(CONNECTIONIMAGE);
            }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    var png = ApplicationExtensions.GetContentByteArray(string.Format("/Resources/Connections/{0}.png", ConnectionImage));
                    _thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(png);
                }
                return _thumbnail;
            }
        }

        private const string ADDRESS = "Address";
        public string Address
        {
            get { return Model.Address; }
            set { Model.Address = value ?? string.Empty; NotifyPropertyChanged(ADDRESS); }
        }

        private const string PORT = "Port";
        public int? Port
        {
            get { return Model.Port; }
            set { Model.Port = value ?? 0; NotifyPropertyChanged(PORT); }
        }

        private const string USERNAME = "Username";
        public string Username
        {
            get { return Model.Username; }
            set { Model.Username = value ?? string.Empty; NotifyPropertyChanged(USERNAME); }
        }

        private const string PASSWORD = "Password";
        public string Password
        {
            get { return Model.Password; }
            set { Model.Password = value ?? string.Empty; NotifyPropertyChanged(PASSWORD); }
        }

        private const string USEPASSIVEMODE = "UsePassiveMode";
        public bool UsePassiveMode
        {
            get { return Model.UsePassiveMode; }
            set { Model.UsePassiveMode = value; NotifyPropertyChanged(USEPASSIVEMODE); }
        }

        public FtpConnectionItemViewModel(FtpConnection model)
        {
            Model = model;
        }

        public FtpConnectionItemViewModel Clone()
        {
            return new FtpConnectionItemViewModel(Model.Clone());
        }
    }
}