using System;
using System.Collections.Generic;
using System.IO;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }

        Queue<FileSystemItem> PopulateQueue();
        Queue<FileSystemItem> PopulateQueue(bool bottomToTop);
        bool CreateFolder(string path);
        bool Delete(FileSystemItem item);

        void GetItemViewModel(string itemPath);
        string GetTargetPath(string sourcePath);

        bool Export(FileSystemItem item, string savePath, CopyAction action);
        bool Import(FileSystemItem item, string savePath, CopyAction action);
    }
}