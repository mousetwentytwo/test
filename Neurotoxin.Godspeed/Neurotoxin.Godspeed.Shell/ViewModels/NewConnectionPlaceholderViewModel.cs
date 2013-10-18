using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class NewConnectionPlaceholderViewModel : ViewModelBase, IStoredConnectionViewModel
    {
        public string Name { get; set; }
        public ImageSource Thumbnail { get; private set; }

        public NewConnectionPlaceholderViewModel()
        {
            var thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/Connections/AddConnection.png");
            Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(thumbnail);
            Name = "New connection...";
        }
    }
}