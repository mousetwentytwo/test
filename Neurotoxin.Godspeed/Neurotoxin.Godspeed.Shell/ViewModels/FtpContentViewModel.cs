using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();

        public FtpConnectionItemViewModel Connection { get; private set; }

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
                                var p = (Tuple<IStoredConnectionViewModel, FileListPaneSettings>) cmdParam;
                                Settings = p.Item2;
                                Connection = (FtpConnectionItemViewModel) p.Item1;
                                return Connect();
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

        private bool Connect()
        {
            FileManager.Connect(Connection.Model);
            return true;
        }

        private void ConnectCallback()
        {
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            var r = new Regex("^/[A-Z0-9]+/", RegexOptions.IgnoreCase);
            var m = r.Match(Settings.Directory);
            FileSystemItemViewModel drive = null;
            if (m.Success)
            {
                drive = Drives.SingleOrDefault(d => d.Path == m.Value);
                if (drive != null) PathCache.Add(drive, Settings.Directory);
            }
            Drive = drive ?? Drives.First();
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

        public bool RemoteDownload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            var c = false;
            switch (action)
            {
                case CopyAction.CreateNew:
                    if (File.Exists(savePath))
                        throw new TransferException(TransferErrorType.WriteAccessError, item.Path, savePath, "Target already exists");
                    break;
                case CopyAction.Resume:
                case CopyAction.Overwrite:
                    c = true;
                    break;
                case CopyAction.OverwriteOlder:
                    var fileDate = File.GetLastWriteTime(savePath);
                    if (fileDate > item.Date) return false;
                    c = true;
                    break;
                default:
                    throw new ArgumentException("Invalid Copy action: " + action);
            }

            var name = RemoteChangeDirectory(item.Path);
            Telnet.Download(name, savePath, c, item.Size ?? 0, (p, t, total) => eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(p, t, total)));
            return true;
        }

        public bool RemoteUpload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            var c = false;
            switch (action)
            {
                case CopyAction.CreateNew:
                    if (FileManager.FileExists(savePath))
                        throw new TransferException(TransferErrorType.WriteAccessError, item.Path, savePath, "Target already exists");
                    break;
                case CopyAction.Resume:
                case CopyAction.Overwrite:
                    c = true;
                    break;
                case CopyAction.OverwriteOlder:
                    var fileDate = FileManager.GetFileModificationTime(savePath);
                    if (fileDate > item.Date) return false;
                    c = true;
                    break;
                default:
                    throw new ArgumentException("Invalid Copy action: " + action);
            }
            var name = RemoteChangeDirectory(savePath);
            Telnet.Upload(item.Path, name, c, item.Size ?? 0, (p, t, total) => eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(p, t, total)));
            return true;
        }

        private string RemoteChangeDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            Telnet.ChangeFtpDirectory(dir);
            return path.Replace(dir, string.Empty);
        }

    }
}