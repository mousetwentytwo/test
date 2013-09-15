using System.Windows.Media;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Presentation.ViewModels
{
    public class Game : ViewModelBase
    {
        //UNDONE
        public string TitleId { get; set; }
        public string Title { get; set; }
        public string Achievements { get; set; }
        public string Gamerscore { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}