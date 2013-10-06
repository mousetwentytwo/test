using System.Collections.Generic;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public interface IPaneViewModel
    {
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }

        Queue<FileSystemItemViewModel> PopulateQueue();
        void SetActive();
        void Refresh();
        bool CreateFolder(string name);
        bool Delete(FileSystemItemViewModel item);
    }
}