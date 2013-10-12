using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Presentation.Attributes;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public class FtpConnectionItemViewModel : ViewModelBase, IStoredConnectionViewModel
    {
        private readonly FtpConnection _model;

        private const string NAME = "Name";
        public string Name
        {
            get { return _model.Name; }
            set { _model.Name = value; NotifyPropertyChanged(NAME); }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    var png = ApplicationExtensions.GetContentByteArray(string.Format("/Resources/Connections/{0}.png", _model.ImageId));
                    _thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(png);
                }
                return _thumbnail;
            }
        }

        private const string ADDRESS = "Address";
        public string Address
        {
            get { return _model.Address; }
            set { _model.Address = value; NotifyPropertyChanged(ADDRESS); }
        }

        private const string PORT = "Port";
        public int Port
        {
            get { return _model.Port; }
            set { _model.Port = value; NotifyPropertyChanged(PORT); }
        }

        private const string USERNAME = "Username";
        public string Username
        {
            get { return _model.Username; }
            set { _model.Username = value; NotifyPropertyChanged(USERNAME); }
        }

        private const string PASSWORD = "Password";
        public string Password
        {
            get { return _model.Password; }
            set { _model.Password = value; NotifyPropertyChanged(PASSWORD); }
        }

        public FtpConnectionItemViewModel(FtpConnection model = null)
        {
            if (model == null) model = new FtpConnection();
            _model = model;
        }

        public void SetImageId(string id)
        {
            _thumbnail = null;
            _model.ImageId = id;
        }
    }
}