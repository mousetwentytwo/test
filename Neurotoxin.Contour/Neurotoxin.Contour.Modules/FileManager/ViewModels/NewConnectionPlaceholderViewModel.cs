using System.Windows.Media;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
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