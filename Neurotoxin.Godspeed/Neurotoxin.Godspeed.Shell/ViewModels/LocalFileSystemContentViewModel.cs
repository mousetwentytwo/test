using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        private const string FREESPACE = "FreeSpace";
        private string _freeSpace;
        public string FreeSpace
        {
            get { return _freeSpace; }
            set { _freeSpace = value; NotifyPropertyChanged(FREESPACE); }
        }

        public LocalFileSystemContentViewModel(FileManagerViewModel parent) : base(parent)
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
                    Drive = Drives.First();
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
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == Drive.Path);
            DriveLabel = string.Format("[{0}]", string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "_NONE_" : driveInfo.VolumeLabel);
            FreeSpace = String.Format("{0:#,0} of {1:#,0} bytes free", driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
            base.ChangeDrive();
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('/', '\\'));
        }

        protected override void SaveToFileStream(string path, FileStream fs, long remoteStartPosition)
        {
            var readStream = File.Open(path, FileMode.Open);
            readStream.Seek(remoteStartPosition, SeekOrigin.Begin);
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fs.Write(buffer, 0, bytesRead);
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
    }
}