using System;
using System.Windows;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{

    public class FtpBrowserViewModel : ModuleViewModelBase
    {
        #region Properties

        private const string FTP = "Ftp";
        private FtpContentViewModel _ftp;
        public FtpContentViewModel Ftp
        {
            get { return _ftp; }
            set { _ftp = value; NotifyPropertyChanged(FTP); }
        }

        private const string LOCALFILESYSTEM = "LocalFileSystem";
        private LocalPaneViewModel _localFileSystem;
        public LocalPaneViewModel LocalFileSystem
        {
            get { return _localFileSystem; }
            set { _localFileSystem = value; NotifyPropertyChanged(LOCALFILESYSTEM); }
        }

        private PaneViewModelBase ActivePane
        {
            get { return Ftp.IsActive ? (PaneViewModelBase) Ftp : (LocalFileSystem.IsActive ? LocalFileSystem : null); }
        }

        #endregion

        public override bool HasDirty()
        {
            throw new NotImplementedException();
        }

        protected override void ResetDirtyFlags()
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty(string propertyName)
        {
            throw new NotImplementedException();
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    Ftp = new FtpContentViewModel(this);
                    Ftp.LoadDataAsync(cmd, cmdParam);
                    LocalFileSystem = new LocalPaneViewModel(this);
                    LocalFileSystem.LoadDataAsync(cmd, cmdParam);
                    break;
            }
        }

        #region EditCommand

        public DelegateCommand<object> EditCommand { get; private set; }

        private void ExecuteEditCommand(object cmdParam)
        {
            MessageBox.Show("Not supported yet");
        }

        private bool CanExecuteEditCommand(object cmdParam)
        {
            return Ftp.IsActive && Ftp.Selection != null;
        }

        #endregion

        #region CopyCommand

        public DelegateCommand<object> CopyCommand { get; private set; }

        private void ExecuteCopyCommand(object cmdParam)
        {
            if (ActivePane == Ftp)
            {
                Ftp.DownloadAll(LocalFileSystem.SelectedPath);
                LocalFileSystem.Refresh();
            } 
            else
            {
                Ftp.UploadAll(LocalFileSystem.Content.Where(item => item.IsSelected).Select(item => item.Path));
                Ftp.Refresh();
            }
        }

        private bool CanExecuteCopyCommand(object cmdParam)
        {
            return ActivePane != null && ActivePane.Selection != null;
        }

        #endregion

        #region MoveCommand

        public DelegateCommand<object> MoveCommand { get; private set; }

        private void ExecuteMoveCommand(object cmdParam)
        {
            CopyCommand.Execute(cmdParam);
            DeleteCommand.Execute(cmdParam);
        }

        private bool CanExecuteMoveCommand(object cmdParam)
        {
            return ActivePane != null && ActivePane.Selection != null;
        }

        #endregion

        #region NewFolderCommand

        public DelegateCommand<object> NewFolderCommand { get; private set; }

        private void ExecuteNewFolderCommand(object cmdParam)
        {
            //UNDONE: pop up a pane dependent input dialog
            var name = "";
            ActivePane.CreateFolder(name);
            ActivePane.Refresh();
        }

        private bool CanExecuteNewFolderCommand(object cmdParam)
        {
            return ActivePane != null;
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand<object> DeleteCommand { get; private set; }

        private void ExecuteDeleteCommand(object cmdParam)
        {
            ActivePane.DeleteAll();
            ActivePane.Refresh();
        }

        private bool CanExecuteDeleteCommand(object cmdParam)
        {
            return ActivePane != null && ActivePane.Selection != null;
        }

        #endregion

        public FtpBrowserViewModel()
        {
            EditCommand = new DelegateCommand<object>(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand<object>(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand<object>(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand<object>(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand<object>(ExecuteDeleteCommand, CanExecuteDeleteCommand);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            EditCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            MoveCommand.RaiseCanExecuteChanged();
            NewFolderCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}