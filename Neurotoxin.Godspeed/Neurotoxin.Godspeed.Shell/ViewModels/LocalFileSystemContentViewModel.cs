using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        private readonly IEventAggregator _eventAggregator;

        public bool IsNetworkDrive
        {
            get { return Drive.FullPath.StartsWith(@"\\"); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override string ExportActionDescription
        {
            get { return null; }
        }

        protected override string ImportActionDescription
        {
            get { return null; }
        }

        public LocalFileSystemContentViewModel(FileManagerViewModel parent, IEventAggregator eventAggregator) : base(parent)
        {
            _eventAggregator = eventAggregator;
            IsResumeSupported = true;
            _eventAggregator.GetEvent<UsbDeviceChangedEvent>().Subscribe(OnUsbDeviceChanged);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
                    var storedDrive = Path.GetPathRoot(Settings.Directory);
                    var drive = Drives.FirstOrDefault(d => d.Path == storedDrive);
                    if (drive != null) PathCache.Add(drive, Settings.Directory);
                    Drive = drive ?? GetDefaultDrive();
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam.Payload as BinaryContent;
                    if (payload == null) return;
                    WorkerThread.Run(
                        () =>
                        {
                            File.WriteAllBytes(payload.FilePath, payload.Content);
                            return true;
                        },
                        result =>
                        {
                            if (success != null) success.Invoke(this);
                        },
                        exception =>
                        {
                            if (error != null) error.Invoke(this, exception);
                        });
                    break;
            }
        }

        private FileSystemItemViewModel GetDefaultDrive()
        {
            if (Drives.Count == 0) return null;
            return Drives.FirstOrDefault(d => d.Name == "C:") ?? Drives.First();
        }


        protected override void ChangeDrive()
        {
            base.ChangeDrive();
            UpdateDriveInfo();
        }

        protected override void ChangeDirectoryCallback(List<FileSystemItem> result)
        {
            base.ChangeDirectoryCallback(result);
            UpdateDriveInfo();
        }

        private void UpdateDriveInfo()
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == Drive.Path);
            DriveLabel = string.Format("[{0}]", string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "_NONE_" : driveInfo.VolumeLabel);
            FreeSpace = String.Format("{0:#,0} of {1:#,0} bytes free", driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('/', '\\'));
        }

        protected override void SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.CopyFile(item.Path, fs, remoteStartPosition);
        }

        protected override Exception WrapTransferRelatedExceptions(Exception exception)
        {
            if (exception is IOException)
                return new TransferException(TransferErrorType.NotSpecified, exception.Message, exception);

            return base.WrapTransferRelatedExceptions(exception);
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        public override void Abort()
        {
            FileManager.AbortCopy();
        }

        private void OnUsbDeviceChanged(UsbDeviceChangedEventArgs e)
        {
            var drives = FileManager.GetDrives();
            for (var i = 0; i < drives.Count; i++)
            {
                var current = drives[i];
                if (i < Drives.Count && current.Name == Drives[i].Name) continue;
                if (i < Drives.Count && Drives.Any(d => d.Name == current.Name))
                {
                    var existing = Drives[i];
                    if (Drive.Name == existing.Name)
                        Drive = GetDefaultDrive();
                    Drives.Remove(existing);
                } 
                else
                {
                    Drives.Insert(i, new FileSystemItemViewModel(current));
                }
            }
        }

        public override void Dispose()
        {
            _eventAggregator.GetEvent<UsbDeviceChangedEvent>().Unsubscribe(OnUsbDeviceChanged);
            base.Dispose();
        }
    }
}