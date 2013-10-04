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
        #region CopyTitleIdToClipboardCommand

        public DelegateCommand<FileSystemItemViewModel> CopyTitleIdToClipboardCommand { get; private set; }

        private void ExecuteCopyTitleIdToClipboardCommand(FileSystemItemViewModel cmdParam)
        {
            Clipboard.SetData(DataFormats.Text, cmdParam.TitleId);
        }

        private bool CanExecuteCopyTitleIdToClipboardCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand<FileSystemItemViewModel> SearchGoogleCommand { get; private set; }

        private void ExecuteSearchGoogleCommand(FileSystemItemViewModel cmdParam)
        {
            System.Diagnostics.Process.Start(string.Format("http://www.google.com/#q={0}", cmdParam.TitleId));
        }

        private bool CanExecuteSearchGoogleCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        #endregion

        #region RenameCommand

        public DelegateCommand<object> RenameCommand { get; private set; }

        private void ExecuteRenameCommand(object cmdParam)
        {
            var grid = cmdParam as DataGrid;
            var row = grid != null ? grid.FindRowByValue(CurrentRow) : cmdParam as DataGridRow;
            if (row == null) return;
            row.FirstCell().IsEditing = true;
            IsInEditMode = true;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteRenameCommand(object cmdParam)
        {
            return true;
        }

        #endregion

        public FtpContentViewModel(ModuleViewModelBase parent, FtpWrapper ftpWrapper) : base(parent, ftpWrapper)
        {
            CopyTitleIdToClipboardCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            RenameCommand = new DelegateCommand<object>(ExecuteRenameCommand, CanExecuteRenameCommand);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
        }

        internal override List<FileSystemItem> ChangeDirectory(string selectedPath = null)
        {
            var recognize = false;
            if (selectedPath == null)
            {
                recognize = true;
                selectedPath = CurrentFolder.Path;
            }

            var content = FileManager.GetList(selectedPath);

            if (recognize)
            {
                foreach (var item in content)
                {
                    switch (item.Type)
                    {
                        case ItemType.Directory:
                            item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                            break;
                        case ItemType.File:
                            item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/file.png");
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    TitleManager.RecognizeTitle(item, CurrentFolder);
                }
            }
            return content;
        }

        protected override long CalculateSize(string path)
        {
            var list = FileManager.GetList(path);
            return list.Where(item => item.Type == ItemType.File).Sum(fi => fi.Size.HasValue ? fi.Size.Value : 0)
                 + list.Where(item => item.Type == ItemType.Directory).Sum(di => CalculateSize(string.Format("{0}{1}/", path, di.TitleId)));
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

        protected override bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            try
            {
                FileManager.FolderExists(drive.Path);
                return true;
            }
            catch
            {
                MessageBox.Show(string.Format("{0} is not accessible.", drive.Title));
                return false;
            }
        }

        protected override void ChangeDrive()
        {
            //TODO: cache
            if (FileManager.FileExists("name.txt"))
            {
                var bytes = FileManager.DownloadFile("name.txt");
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
            var drives = FileManager.GetList();
            //TODO: switch
            drives.ForEach(d => d.Type = ItemType.Drive);
            Drives = drives.Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
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

        public override bool Delete(FileSystemItemViewModel item)
        {
            //TODO: handle errors

            if (item.Type == ItemType.Directory)
            {
                FileManager.DeleteFolder(item.Path);
            } 
            else
            {
                FileManager.DeleteFile(item.Path);
            }
            return true;
        }

        public override bool CreateFolder(string name)
        {
            var path = string.Format("{0}{1}", CurrentFolder.Path, name);
            FileManager.CreateFolder(path);
            return true;
        }


    }
}