using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        FileSystemItemViewModel Drive { get; set; }
        long? FreeSpace { get; }

        ObservableCollection<FileSystemItemViewModel> Items { get; }
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }
        ResumeCapability ResumeCapability { get; }
        bool HasValidSelection { get; }
        bool IsReadOnly { get; }
        bool IsInEditMode { get; }
        bool IsFSD { get; }
        bool IsVerificationEnabled { get; }

        Queue<QueueItem> PopulateQueue(FileOperation action);
        Queue<QueueItem> PopulateQueue(FileOperation action, IEnumerable<FileSystemItem> selection);
        TransferResult CreateFolder(string path);
        TransferResult Delete(FileSystemItem item);

        void GetItemViewModel(string itemPath);
        string GetTargetPath(string sourcePath);

        //TransferResult Export(FileSystemItem item, string savePath, CopyAction action);
        //TransferResult Import(FileSystemItem item, string savePath, CopyAction action);

        FileExistenceInfo FileExists(string path);
        Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition);
        bool CopyStream(FileSystemItem item, Stream stream, long remoteStartPosition = 0, long? byteLimit = null);

        void Refresh();
        void Refresh(Action callback);
        void Abort();

        void Recognize(FileSystemItemViewModel item);
    }
}