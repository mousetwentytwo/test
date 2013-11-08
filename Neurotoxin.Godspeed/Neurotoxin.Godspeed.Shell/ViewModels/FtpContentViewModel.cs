using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();

        #region DisconnectCommand

        public DelegateCommand DisconnectCommand { get; private set; }

        private void ExecuteDisconnectCommand()
        {
            FileManager.Disconnect();
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        public FtpContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            DisconnectCommand = new DelegateCommand(ExecuteDisconnectCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                            {
                                return Connect((FtpConnectionItemViewModel) cmdParam);
                            },
                        result =>
                            {
                                ConnectCallback();
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam as byte[];
                    if (payload == null) return;
                    WorkerThread.Run(
                        () =>
                            {
                                File.WriteAllBytes(CurrentRow.TempFilePath, payload);
                                FileManager.RestoreConnection();
                                FileManager.UploadFile(CurrentRow.Path, CurrentRow.TempFilePath);
                                return true;
                            },
                        result =>
                            {
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                DisconnectCommand.Execute();
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
            }
        }

        private FtpConnectionItemViewModel Connect(FtpConnectionItemViewModel connection)
        {
            FileManager.Connect(connection.Model);
            return connection;
        }

        private void ConnectCallback()
        {
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            Drive = Drives.SingleOrDefault(d => d.Name == "Hdd1") ?? Drives.First();
        }

        public void RestoreConnection()
        {
            FileManager.RestoreConnection();
        }

        public override void Abort()
        {
            FileManager.Abort();
        }

        protected override void ChangeDrive()
        {
            if (!_driveLabelCache.ContainsKey(Drive.Path))
            {
                var path = String.Format("{0}name.txt", Drive.Path);
                string label = null;
                if (FileManager.FileExists(path))
                {
                    var bytes = FileManager.ReadFileContent(path);
                    label = String.Format("[{0}]", Encoding.BigEndianUnicode.GetString(bytes));
                }
                _driveLabelCache.Add(Drive.Path, label);
            }
            DriveLabel = _driveLabelCache[Drive.Path];
            base.ChangeDrive();
        }

        public override string GetTargetPath(string path)
        {
            return String.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override void SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.DownloadFile(item.Path, fs, remoteStartPosition, item.Size ?? 0);
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            FileManager.UploadFile(targetPath, sourcePath);
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            FileManager.UploadFile(targetPath, sourcePath);
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
        {
            FileManager.AppendFile(targetPath, sourcePath);
        }
    }
}