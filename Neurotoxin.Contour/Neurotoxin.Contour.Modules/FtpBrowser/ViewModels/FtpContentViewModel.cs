using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FtpContentViewModel : PaneViewModelBase<FtpWrapper>
    {

        public FtpContentViewModel(ModuleViewModelBase parent) : base(parent, new FtpWrapper())
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(Connect, ConnectCallback);
                    break;
            }
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

        private bool Connect()
        {
            return FileManager.Connect();
        }

        private void ConnectCallback(bool success)
        {
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            Drive = Drives.First();
        }

        public bool Download(FileSystemItemViewModel ftpItem, string targetPath, CopyBehavior behavior, bool overwriteOlderOnly)
        {
            var remotePath = ftpItem.Path;
            var localPath = remotePath.Replace(CurrentFolder.Path, targetPath).Replace('/', '\\');

            FileMode mode;
            switch (behavior)
            {
                case CopyBehavior.Default:
                    mode = FileMode.CreateNew;
                    break;
                case CopyBehavior.Overwrite:
                    mode = FileMode.Create;
                    break;
                case CopyBehavior.Resume:
                    mode = FileMode.Append;
                    break;
                default:
                    throw new ArgumentException("Invalid Copy behavior: " + behavior);
            }

            switch (ftpItem.Type)
            {
                case ItemType.File:
                    if (behavior == CopyBehavior.Overwrite && overwriteOlderOnly)
                    {
                        var fileDate = File.GetLastWriteTime(localPath);
                        if (fileDate > ftpItem.Date) return false;
                    }
                    var fs = new FileStream(localPath, mode);
                    long remoteStartPosition = 0;
                    if (behavior == CopyBehavior.Resume)
                    {
                        var fi = new FileInfo(localPath);
                        remoteStartPosition = fi.Length;
                    }
                    FileManager.DownloadFile(remotePath, fs, remoteStartPosition);
                    fs.Flush();
                    fs.Close();
                    break;
                case ItemType.Directory:
                    if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }

        public bool Upload(FileSystemItemViewModel localItem, string sourcePath, CopyBehavior behavior, bool overwriteOlderOnly)
        {
            var localPath = localItem.Path;
            var remotePath = localPath.Replace(sourcePath, CurrentFolder.Path).Replace('\\', '/');

            switch (localItem.Type)
            {
                case ItemType.File:
                    if (behavior == CopyBehavior.Overwrite && overwriteOlderOnly)
                    {
                        var fileDate = FileManager.GetFileModificationTime(remotePath);
                        if (fileDate > localItem.Date) return false;
                    }
                    switch (behavior)
                    {
                        case CopyBehavior.Default:
                            if (FileManager.FileExists(remotePath))
                                throw new Exception("Target already exists");
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyBehavior.Overwrite:
                            //TODO: assumption only!
                            FileManager.UploadFile(remotePath, localPath);
                            break;
                        case CopyBehavior.Resume:
                            FileManager.AppendFile(remotePath, localPath);
                            break;
                        default:
                            throw new ArgumentException("Invalid Copy behavior: " + behavior);
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
    }
}