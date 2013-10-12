using System.Collections.Generic;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }

        Queue<FileSystemItemViewModel> PopulateQueue();
        bool CreateFolder(string name);
        bool Delete(FileSystemItemViewModel item);
        FileSystemItemViewModel GetItemViewModel(string itemPath);
    }
}