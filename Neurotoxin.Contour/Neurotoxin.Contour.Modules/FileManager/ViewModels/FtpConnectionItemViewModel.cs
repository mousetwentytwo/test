using System.Windows.Media;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
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

        private const string XBOXVERSION = "XboxVersion";
        public ConnectionImage ConnectionImage
        {
            get { return Model.ConnectionImage; }
            set
            {
                Model.ConnectionImage = value;
                _thumbnail = null;
                NotifyPropertyChanged(XBOXVERSION);
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
                    var png = ApplicationExtensions.GetContentByteArray(string.Format("/Resources/Connections/{0}.png", Model.ConnectionImage));
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

        public FtpConnectionItemViewModel(FtpConnection model = null)
        {
            if (model == null) model = new FtpConnection();
            Model = model;
        }

        public FtpConnectionItemViewModel Clone()
        {
            return new FtpConnectionItemViewModel(Model.Clone());
        }
    }
}