using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
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
                    Ftp = new FtpContentViewModel(this);
                    Ftp.FileManager.FtpOperationStarted += FtpWrapperOnFtpOperationStarted;
                    Ftp.FileManager.FtpOperationFinished += FtpWrapperOnFtpOperationFinished;
                    Ftp.FileManager.FtpOperationProgressChanged += FtpWrapperOnFtpOperationProgressChanged;
                    Ftp.LoadDataAsync(cmd, cmdParam);
                    LocalFileSystem = new LocalPaneViewModel(this);
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

        #region SwitchPaneCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SwitchPaneCommand { get; private set; }

        private bool CanExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            return eventInformation.EventArgs.Key == Key.Tab;
        }

        private void ExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            TargetPane.SetActive();
            eventInformation.EventArgs.Handled = true;
        }

        #endregion

        #region EditCommand

        public DelegateCommand EditCommand { get; private set; }

        private bool CanExecuteEditCommand()
        {
            return true;
        }
        
        private void ExecuteEditCommand()
        {
            MessageBox.Show("Not supported yet");
        }

        #endregion

        #region CopyCommand

        private Queue<FileSystemItemViewModel> _queue;

        public DelegateCommand CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand()
        {
            return SourcePane != null && (SourcePane.SelectedItems.Any() || SourcePane.CurrentRow != null);
        }

        private void ExecuteCopyCommand()
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

        public DelegateCommand MoveCommand { get; private set; }

        private bool CanExecuteMoveCommand()
        {
            return SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteMoveCommand()
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

        public DelegateCommand NewFolderCommand { get; private set; }

        private bool CanExecuteNewFolderCommand()
        {
            return SourcePane != null;
        }

        private void ExecuteNewFolderCommand()
        {
            //UNDONE: pop up a pane dependent input dialog
            var name = "";
            SourcePane.CreateFolder(name);
            SourcePane.Refresh();
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand DeleteCommand { get; private set; }

        private bool CanExecuteDeleteCommand()
        {
            return SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteDeleteCommand()
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

        #endregion

        public FtpBrowserViewModel()
        {
            SwitchPaneCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSwitchPaneCommand, CanExecuteSwitchPaneCommand);
            EditCommand = new DelegateCommand(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
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