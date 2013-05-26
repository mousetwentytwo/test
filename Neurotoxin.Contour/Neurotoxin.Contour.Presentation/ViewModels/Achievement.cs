using System;
using System.Windows.Media;

namespace Neurotoxin.Contour.Presentation.ViewModels
{
    public class Achievement
    {
        public int AchievementId { get; set; }
        public ImageSource Thumbnail { get; set; }
        public int Gamerscore { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public DateTime UnlockTime { get; set; }
    }
}