using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        public bool Download(FileSystemItemViewModel ftpItem, string targetPath)
        {
            //TODO: append / rewrite / etc. options

            var remotePath = ftpItem.Path;
            var localPath = remotePath.Replace(CurrentFolder.Path, targetPath).Replace('/', '\\');

            switch (ftpItem.Type)
            {
                case ItemType.File:
                    var fs = new FileStream(localPath, FileMode.OpenOrCreate);
                    FileManager.DownloadFile(remotePath, localPath);
                    fs.Flush();
                    fs.Close();
                    break;
                case ItemType.Directory:
                    Directory.CreateDirectory(localPath);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }

        public bool Upload(FileSystemItemViewModel localItem, string sourcePath)
        {
            //TODO: append / rewrite / etc. options

            var localPath = localItem.Path;
            var remotePath = localPath.Replace(sourcePath, CurrentFolder.Path).Replace('\\', '/');

            switch (localItem.Type)
            {
                case ItemType.File:
                    FileManager.UploadFile(remotePath, localPath);
                    break;
                case ItemType.Directory:
                    FileManager.CreateFolder(remotePath);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }
    }
}