using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface ITreeSelectionViewModel
    {
        string TreeSelectionTitle { get; }
        ObservableCollection<TreeItemViewModel> SelectionTree { get; set; }
        IViewModel Parent { get; }
    }
}