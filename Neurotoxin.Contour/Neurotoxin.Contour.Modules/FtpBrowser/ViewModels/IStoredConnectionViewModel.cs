using System.Windows.Media;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public interface IStoredConnectionViewModel
    {
        string Name { get; set; }
        ImageSource Thumbnail { get; }
    }
}