using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Exceptions;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpWrapper>
    {

        #region DisconnectCommand

        public DelegateCommand<EventInformation<EventArgs>> DisconnectCommand { get; private set; }

        private void ExecuteDisconnectCommand(EventInformation<EventArgs> cmdParam)
        {
            FileManager.Disconnect();
            //TODO: via event aggregator
            ((FtpBrowserViewModel)Parent).FtpDisconnect();
        }

        #endregion

        public FtpContentViewModel(ModuleViewModelBase parent) : base(parent, new FtpWrapper())
        {
            DisconnectCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteDisconnectCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action success = null, Action error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(() => Connect((FtpConnectionItemViewModel)cmdParam), (r) =>
                        {
                            ConnectCallback(r);
                            if (r && success != null) success.Invoke();
                            if (!r && error != null) error.Invoke();
                        });
                    break;
            }
        }

        private bool Connect(FtpConnectionItemViewModel connection)
        {
            return FileManager.Connect(connection.Address, connection.Port, connection.Username, connection.Password);
        }

        private void ConnectCallback(bool success)
        {
            if (!success) return;
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            Drive = Drives.First();
        }

        protected override void ChangeDrive()
        {
            //TODO: cache
            var path = string.Format("{0}name.txt", Drive.Path);
            if (FileManager.FileExists(path))
            {
                var bytes = FileManager.DownloadFile(path);
                DriveLabel = string.Format("[{0}]", System.Text.Encoding.BigEndianUnicode.GetString(bytes));
            }
            else
            {
                DriveLabel = null;
            }
            base.ChangeDrive();
        }

        public bool Download(FileSystemItemViewModel ftpItem, string targetPath, CopyAction action)
        {
            var remotePath = ftpItem.Path;
            var localPath = remotePath.Replace(CurrentFolder.Path, targetPath).Replace('/', '\\');

            FileMode mode;
            long remoteStartPosition = 0;
            switch (action)
            {
                case CopyAction.CreateNew:
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
                default:
                    throw new ArgumentException("Invalid Copy action: " + action);
            }

            switch (ftpItem.Type)
            {
                case ItemType.File:
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

        public bool Upload(FileSystemItemViewModel localItem, string sourcePath, CopyAction action)
        {
            var localPath = localItem.Path;
            var remotePath = localPath.Replace(sourcePath, CurrentFolder.Path).Replace('\\', '/');

            switch (localItem.Type)
            {
                case ItemType.File:
                    switch (action)
                    {
                        case CopyAction.CreateNew:
                            if (FileManager.FileExists(remotePath))
                                throw new TransferException(TransferErrorType.WriteAccessError, "Target already exists");
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyAction.Overwrite:
                            //TODO: assumption only!
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