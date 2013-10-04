using System;
using System.Collections.Generic;
using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
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

        private IPaneViewModel SourcePane
        {
            get { return Ftp.IsActive ? (IPaneViewModel)Ftp : (LocalFileSystem.IsActive ? LocalFileSystem : null); }
        }

        private IPaneViewModel TargetPane
        {
            get { return SourcePane == Ftp ? (IPaneViewModel)LocalFileSystem : SourcePane == LocalFileSystem ? Ftp : null; }
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
                    Ftp = new FtpContentViewModel(this, new FtpWrapper());
                    Ftp.FileManager.FtpOperationStarted += FtpWrapperOnFtpOperationStarted;
                    Ftp.FileManager.FtpOperationFinished += FtpWrapperOnFtpOperationFinished;
                    Ftp.FileManager.FtpOperationProgressChanged += FtpWrapperOnFtpOperationProgressChanged;
                    Ftp.LoadDataAsync(cmd, cmdParam);
                    LocalFileSystem = new LocalPaneViewModel(this, new LocalWrapper());
                    LocalFileSystem.LoadDataAsync(cmd, cmdParam);
                    break;
            }
        }

        private void FtpWrapperOnFtpOperationStarted(object sender, FtpOperationStartedEventArgs args)
        {
            UIThread.Run(() =>
            {
                IsInProgress = true;
                //TODO: support indetermine progressbar
                LoadingQueueLength = args.BinaryTransfer ? 100 : 1;
                LoadingProgress = 0;
            });
        }

        private void FtpWrapperOnFtpOperationFinished(object sender, FtpOperationFinishedEventArgs args)
        {
            UIThread.Run(() => IsInProgress = false);
        }

        private void FtpWrapperOnFtpOperationProgressChanged(object sender, FtpOperationProgressChangedEventArgs args)
        {
            UIThread.Run(() => LoadingProgress = args.Percentage);
        }

        #region EditCommand

        public DelegateCommand<object> EditCommand { get; private set; }

        private void ExecuteEditCommand(object cmdParam)
        {
            MessageBox.Show("Not supported yet");
        }

        private bool CanExecuteEditCommand(object cmdParam)
        {
            return Ftp.IsActive && Ftp.CurrentRow != null;
        }

        #endregion

        #region CopyCommand

        private Queue<FileSystemItemViewModel> _queue;
//        private TransferQueue _transferQueue;

        public DelegateCommand<object> CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand(object cmdParam)
        {
            return SourcePane != null && (SourcePane.SelectedItems.Any() || SourcePane.CurrentRow != null);
        }

        private void ExecuteCopyCommand(object cmdParam)
        {
            _queue = SourcePane.PopulateQueue();
            StartCopy();
        }

        private void StartCopy()
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => Ftp.Download(item, TargetPane.CurrentFolder.Path), EndCopy);
                }
                else
                {
                    WorkerThread.Run(() => Ftp.Upload(item, SourcePane.CurrentFolder.Path), EndCopy);
                } 
            }
            else
            {
                FinishCopy();
                _queue = null;
            }            
        }

        private void EndCopy(bool result)
        {
            var item = _queue.Dequeue();
            if (result) item.IsSelected = false;
            StartCopy();
        }

        private void FinishCopy()
        {
            TargetPane.Refresh();
        }

        #endregion

        #region MoveCommand

        public DelegateCommand<object> MoveCommand { get; private set; }

        private bool CanExecuteMoveCommand(object cmdParam)
        {
            return SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteMoveCommand(object cmdParam)
        {
            _queue = SourcePane.PopulateQueue();
            StartMove();
        }

        private void StartMove()
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => { Ftp.Download(item, TargetPane.CurrentFolder.Path); Ftp.Delete(item); return true; }, EndMove);
                }
                else
                {
                    WorkerThread.Run(() => { Ftp.Upload(item, SourcePane.CurrentFolder.Path); LocalFileSystem.Delete(item); return true; }, EndMove);
                }
            }
            else
            {
                FinishMove();
                _queue = null;
            }
        }

        private void EndMove(bool result)
        {
            var item = _queue.Dequeue();
            if (result) item.IsSelected = false;
            StartMove();
        }

        private void FinishMove()
        {
            SourcePane.Refresh();
            TargetPane.Refresh();
        }

        #endregion

        #region NewFolderCommand

        public DelegateCommand<object> NewFolderCommand { get; private set; }

        private void ExecuteNewFolderCommand(object cmdParam)
        {
            //UNDONE: pop up a pane dependent input dialog
            var name = "";
            SourcePane.CreateFolder(name);
            SourcePane.Refresh();
        }

        private bool CanExecuteNewFolderCommand(object cmdParam)
        {
            return SourcePane != null;
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand<object> DeleteCommand { get; private set; }

        private void ExecuteDeleteCommand(object cmdParam)
        {
            _queue = new Queue<FileSystemItemViewModel>(SourcePane.SelectedItems.Any() ? SourcePane.SelectedItems : new[] { SourcePane.CurrentRow });
            StartDelete();
        }

        private void StartDelete()
        {
            if (_queue.Count > 0)
            {
                WorkerThread.Run(() => SourcePane.Delete(_queue.Peek()), EndDelete);
            }
            else
            {
                FinishDelete();
                _queue = null;
            }
        }

        private void EndDelete(bool result)
        {
            var item = _queue.Dequeue();
            if (result) item.IsSelected = false;
            StartDelete();
        }

        private void FinishDelete()
        {
            SourcePane.Refresh();
        }

        private bool CanExecuteDeleteCommand(object cmdParam)
        {
            return SourcePane != null && SourcePane.CurrentRow != null;
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