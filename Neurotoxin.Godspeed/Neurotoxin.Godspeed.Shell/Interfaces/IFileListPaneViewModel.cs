using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }

        Queue<FileSystemItemViewModel> PopulateQueue();
        Queue<FileSystemItemViewModel> PopulateQueue(bool bottomToTop);
        bool CreateFolder(string name);
        bool Delete(FileSystemItemViewModel item);

        void GetItemViewModel(string itemPath);
    }
}