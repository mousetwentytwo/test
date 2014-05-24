using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileListPaneViewModel : IPaneViewModel
    {
        FileSystemItemViewModel Drive { get; set; }

        ObservableCollection<FileSystemItemViewModel> Items { get; }
        IEnumerable<FileSystemItemViewModel> SelectedItems { get; }
        FileSystemItemViewModel CurrentFolder { get; }
        FileSystemItemViewModel CurrentRow { get; set; }
        bool IsResumeSupported { get; }
        bool HasValidSelection { get; }
        bool IsReadOnly { get; }
        bool IsInEditMode { get; }
        bool IsVerificationSupported { get; }
        bool IsVerificationEnabled { get; }

        Queue<QueueItem> PopulateQueue(FileOperation action);
        TransferResult CreateFolder(string path);
        TransferResult Delete(FileSystemItem item);

        void GetItemViewModel(string itemPath);
        string GetTargetPath(string sourcePath);

        TransferResult Export(FileSystemItem item, string savePath, CopyAction action);
        TransferResult Import(FileSystemItem item, string savePath, CopyAction action);

        void Refresh();
        void Refresh(Action callback);
        void Abort();
        void FinishTransferAsSource();
        void FinishTransferAsTarget();
    }
}