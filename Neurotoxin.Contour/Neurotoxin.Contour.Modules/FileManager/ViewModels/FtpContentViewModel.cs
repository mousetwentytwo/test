using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.ContentProviders;
using Neurotoxin.Contour.Modules.FileManager.Exceptions;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private const string RenameFromPattern = @"([\/]){0}$";
        private const string RenameToPattern = @"$1{0}";
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();

        #region DisconnectCommand

        public DelegateCommand<EventInformation<EventArgs>> DisconnectCommand { get; private set; }

        private void ExecuteDisconnectCommand(EventInformation<EventArgs> cmdParam)
        {
            FileManager.Disconnect();
            //TODO: via event aggregator
            Parent.FtpDisconnect();
        }

        #endregion

        public FtpContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            DisconnectCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteDisconnectCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(() => Connect((FtpConnectionItemViewModel)cmdParam), (r) =>
                        {
                            ConnectCallback(r);
                            if (r && success != null) success.Invoke(this);
                            if (!r && error != null) error.Invoke(this);
                        });
                    break;
            }
        }

        private bool Connect(FtpConnectionItemViewModel connection)
        {
            return FileManager.Connect(connection.Model);
        }

        private void ConnectCallback(bool success)
        {
            if (!success) return;
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            Drive = Drives.First();
        }

        protected override void ChangeDrive()
        {
            if (!_driveLabelCache.ContainsKey(Drive.Path))
            {
                var path = string.Format("{0}name.txt", Drive.Path);
                string label = null;
                if (FileManager.FileExists(path))
                {
                    var bytes = FileManager.DownloadFile(path);
                    label = string.Format("[{0}]", System.Text.Encoding.BigEndianUnicode.GetString(bytes));
                }
                _driveLabelCache.Add(Drive.Path, label);
            }
            DriveLabel = _driveLabelCache[Drive.Path];
            base.ChangeDrive();
        }

        public bool Download(FileSystemItemViewModel ftpItem, string targetPath, CopyAction action, string rename)
        {
            var remotePath = ftpItem.Path;
            var localPath = remotePath.Replace(CurrentFolder.Path, targetPath).Replace('/', '\\');

            switch (ftpItem.Type)
            {
                case ItemType.File:
                    FileMode mode;
                    long remoteStartPosition = 0;
                    switch (action)
                    {
                        case CopyAction.CreateNew:
                            if (FileManager.FileExists(localPath))
                                throw new TransferException(TransferErrorType.WriteAccessError, remotePath, localPath, "Target already exists");
                            mode = FileMode.CreateNew;
                            break;
                        case CopyAction.Overwrite:
                            mode = FileMode.Create;
                            break;
                        case CopyAction.OverwriteOlder:
                            var fileDate = File.GetLastWriteTime(localPath);
                            if (fileDate > ftpItem.Date) return false;
                            mode = FileMode.Create;
                            break;
                        case CopyAction.Resume:
                            mode = FileMode.Append;
                            var fi = new FileInfo(localPath);
                            remoteStartPosition = fi.Length;
                            break;
                        case CopyAction.Rename:
                            mode = FileMode.CreateNew;
                            var r = new Regex(string.Format(RenameFromPattern, ftpItem.Name), RegexOptions.IgnoreCase);
                            localPath = r.Replace(localPath, string.Format(RenameToPattern, rename));
                            break;
                        default:
                            throw new ArgumentException("Invalid Copy action: " + action);
                    }
                    FileManager.DownloadFile(remotePath, localPath, mode, remoteStartPosition);
                    break;
                case ItemType.Directory:
                    if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }

        public bool Upload(FileSystemItemViewModel localItem, string sourcePath, CopyAction action, string rename)
        {
            var localPath = localItem.Path;
            var remotePath = localPath.Replace(sourcePath, CurrentFolder.Path).Replace('\\', '/');

            switch (localItem.Type)
            {
                case ItemType.File:
                    if (action == CopyAction.Rename)
                    {
                        var r = new Regex(string.Format(RenameFromPattern, localItem.Name), RegexOptions.IgnoreCase);
                        remotePath = r.Replace(remotePath, string.Format(RenameToPattern, rename));
                        action = CopyAction.CreateNew;
                    }

                    switch (action)
                    {
                        case CopyAction.CreateNew:
                            if (FileManager.FileExists(remotePath))
                                throw new TransferException(TransferErrorType.WriteAccessError, localPath, remotePath, "Target already exists");
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyAction.Overwrite:
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyAction.OverwriteOlder:
                            var fileDate = FileManager.GetFileModificationTime(remotePath);
                            if (fileDate > localItem.Date) return false;
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyAction.Resume:
                            FileManager.AppendFile(remotePath, localPath);
                            break;
                        default:
                            throw new ArgumentException("Invalid Copy action: " + action);
                    }
                    break;
                case ItemType.Directory:
                    if (!FileManager.FolderExists(remotePath)) FileManager.CreateFolder(remotePath);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }

        public void RestoreConnection()
        {
            FileManager.RestoreConnection();
        }
    }
}