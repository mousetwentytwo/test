using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

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
            Name = Resx.NewConnection + Strings.DotDotDot;
        }
    }
}