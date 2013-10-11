using System.Collections.Generic;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }

        Queue<FileSystemItemViewModel> PopulateQueue();
        bool CreateFolder(string name);
        bool Delete(FileSystemItemViewModel item);
    }
}