using System.Windows.Media;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IStoredConnectionViewModel
    {
        string Name { get; set; }
        ImageSource Thumbnail { get; }
    }
}