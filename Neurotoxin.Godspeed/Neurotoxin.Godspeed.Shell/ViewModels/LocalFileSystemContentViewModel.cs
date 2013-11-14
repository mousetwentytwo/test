using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        private bool _isAborted;

        private const string FREESPACE = "FreeSpace";
        private string _freeSpace;
        public string FreeSpace
        {
            get { return _freeSpace; }
            set { _freeSpace = value; NotifyPropertyChanged(FREESPACE); }
        }

        public bool IsNetworkDrive
        {
            get { return Drive.FullPath.StartsWith(@"\\"); }
        }

        public LocalFileSystemContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            IsResumeSupported = true;
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
                    var storedDrive = Path.GetPathRoot(Settings.Directory);
                    var drive = Drives.FirstOrDefault(d => d.Path == storedDrive);
                    if (drive != null) PathCache.Add(drive, Settings.Directory);
                    Drive = drive ?? Drives.First();
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam as byte[];
                    if (payload == null) return;
                    WorkerThread.Run(
                        () =>
                        {
                            File.WriteAllBytes(CurrentRow.Path, payload);
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
            _isAborted = false;
            var totalBytesTransferred = remoteStartPosition;
            var readStream = File.Open(item.Path, FileMode.Open);
            var totalBytes = readStream.Length;
            readStream.Seek(remoteStartPosition, SeekOrigin.Begin);
            var buffer = new byte[32768];
            int bytesRead;
            eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(-1, remoteStartPosition, remoteStartPosition, remoteStartPosition));
            while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0 && !_isAborted)
            {
                fs.Write(buffer, 0, bytesRead);
                totalBytesTransferred += bytesRead;
                var percentage = (int)(totalBytesTransferred/totalBytes*100);
                eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, bytesRead, totalBytesTransferred, remoteStartPosition));
            }
            readStream.Close();
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            throw new NotImplementedException();
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            throw new NotImplementedException();
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
        {
            throw new NotImplementedException();
        }

        public override void Abort()
        {
            _isAborted = true;
        }
    }
}