using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Presentation.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ITreeSelectionViewModel
    {
        ObservableCollection<TreeItemViewModel> SelectionTree { get; set; }
    }
}
