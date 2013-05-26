using System.Windows.Media;

namespace Neurotoxin.Contour.Presentation.ViewModels
{
    public class Game
    {
        public string TitleId { get; set; }
        public string Title { get; set; }
        public string Achievements { get; set; }
        public string Gamerscore { get; set; }
        public ImageSource Thumbnail { get; set; }
    }
}