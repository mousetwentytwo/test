using System.Windows.Media;
using Neurotoxin.Contour.Modules.FtpBrowser.Interfaces;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class NewConnectionPlaceholderViewModel : ViewModelBase, IStoredConnectionViewModel
    {
        public string Name { get; set; }
        public ImageSource Thumbnail { get; private set; }

        public NewConnectionPlaceholderViewModel()
        {
            var thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/Connections/add_connection.png");
            Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(thumbnail);
            Name = "New connection...";
        }
    }
}