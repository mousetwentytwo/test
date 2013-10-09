﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.Views;
using Neurotoxin.Contour.Presentation.Events;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{

    public class FtpBrowserViewModel : ModuleViewModelBase
    {
        private Queue<FileSystemItemViewModel> _queue;
        private CopyBehavior _copyBehavior;
        private bool _overwriteOlder;

        #region Main window properties

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

        #region Transfer properties

        private const string ACTIONLABEL = "ActionLabel";
        private string _actionLabel;
        public string ActionLabel
        {
            get { return _actionLabel; }
            set { _actionLabel = value; NotifyPropertyChanged(ACTIONLABEL); }
        }

        private const string SOURCEFILE = "SourceFile";
        private string _sourceFile;
        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; NotifyPropertyChanged(SOURCEFILE); }
        }

        private const string TARGETFILE = "TargetFile";
        private string _targetFile;
        public string TargetFile
        {
            get { return _targetFile; }
            set { _targetFile = value; NotifyPropertyChanged(TARGETFILE); }
        }

        private const string CURRENTFILEPROGRESS = "CurrentFileProgress";
        private int _currentFileProgress;
        public int CurrentFileProgress
        {
            get { return _currentFileProgress; }
            set { _currentFileProgress = value; NotifyPropertyChanged(CURRENTFILEPROGRESS); }
        }

        private const string TOTALPROGRESS = "TotalProgress";
        public int TotalProgress
        {
            get { return (int)(BytesTransfered * 100 / TotalBytes); }
        }

        private const string FILESTRANSFERED = "FilesTransfered";
        private int _filesTransfered;
        public int FilesTransfered
        {
            get { return _filesTransfered; }
            set { _filesTransfered = value; NotifyPropertyChanged(FILESTRANSFERED); }
        }

        private const string FILECOUNT = "FileCount";
        private int _fileCount;
        public int FileCount
        {
            get { return _fileCount; }
            set { _fileCount = value; NotifyPropertyChanged(FILECOUNT); }
        }

        private const string BYTESTRANSFERED = "BytesTransfered";
        private long _bytesTransfered;
        public long BytesTransfered
        {
            get { return _bytesTransfered; }
            set
            {
                _bytesTransfered = value;
                NotifyPropertyChanged(BYTESTRANSFERED);
                NotifyPropertyChanged(TOTALPROGRESS);
            }
        }

        private const string TOTALBYTES = "TotalBytes";
        private long _totalBytes;
        public long TotalBytes
        {
            get { return _totalBytes; }
            set { _totalBytes = value; NotifyPropertyChanged(TOTALBYTES); }
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
            UIThread.Run(() =>
                {
                    LoadingProgress = args.Percentage;
                    CurrentFileProgress = args.Percentage;
                });
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

        public DelegateCommand CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand()
        {
            return SourcePane != null && (SourcePane.SelectedItems.Any() || SourcePane.CurrentRow != null);
        }

        private void ExecuteCopyCommand()
        {
            _queue = SourcePane.PopulateQueue();
            InitializeTransfer("Copy:");
            CopyStart();
        }

        private void CopyStart(CopyBehavior? behavior = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => Ftp.Download(item, TargetPane.CurrentFolder.Path, behavior ?? _copyBehavior, _overwriteOlder), CopySuccess, CopyError);
                }
                else
                {
                    WorkerThread.Run(() => Ftp.Upload(item, SourcePane.CurrentFolder.Path, behavior ?? _copyBehavior, _overwriteOlder), CopySuccess, CopyError);
                } 
            }
            else
            {
                CopyFinish();
                _queue = null;
            }            
        }

        private void CopySuccess(bool result)
        {
            UpdateTransfer(result);
            CopyStart();
        }

        private void CopyError(Exception exception)
        {
            ShowTransferErrorDialog(exception, CopyStart, CopySuccess, CopyFinish);
        }

        private void CopyFinish()
        {
            FinishTransfer();
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
            InitializeTransfer("Move:");
            MoveStart();
        }

        private void MoveStart(CopyBehavior? behavior = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => { Ftp.Download(item, TargetPane.CurrentFolder.Path, behavior ?? _copyBehavior, _overwriteOlder); Ftp.Delete(item); return true; }, MoveSuccess, MoveError);
                }
                else
                {
                    WorkerThread.Run(() => { Ftp.Upload(item, SourcePane.CurrentFolder.Path, behavior ?? _copyBehavior, _overwriteOlder); LocalFileSystem.Delete(item); return true; }, MoveSuccess, MoveError);
                }
            }
            else
            {
                MoveFinish();
                _queue = null;
            }
        }

        private void MoveSuccess(bool result)
        {
            UpdateTransfer(result);
            MoveStart();
        }

        private void MoveError(Exception exception)
        {
            ShowTransferErrorDialog(exception, MoveStart, MoveSuccess, MoveFinish);
        }

        private void MoveFinish()
        {
            FinishTransfer();
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
            DeleteStart();
        }

        private void DeleteStart()
        {
            if (_queue.Count > 0)
            {
                WorkerThread.Run(() => SourcePane.Delete(_queue.Peek()), DeleteSuccess, DeleteError);
            }
            else
            {
                DeleteFinish();
                _queue = null;
            }
        }

        private void DeleteSuccess(bool result)
        {
            UpdateTransfer(result);
            var item = _queue.Dequeue();
            if (result) item.IsSelected = false;
            DeleteStart();
        }

        private void DeleteError(Exception exception)
        {
            throw new NotImplementedException();
        }

        private void DeleteFinish()
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

        private void InitializeTransfer(string actionLabel)
        {
            FileCount = _queue.Count;
            TotalBytes = _queue.Where(item => item.Type == ItemType.File).Sum(item => item.Size ?? 0);
            IsInProgress = true;
            ActionLabel = actionLabel;
            eventAggregator.GetEvent<TransferStartedEvent>().Publish(new TransferStartedEventArgs());
        }

        private void ShowTransferErrorDialog(Exception exception, Action<CopyBehavior?> retry, Action<bool> skip, Action cancel)
        {
            var dialog = new TransferErrorDialog(exception);
            var result = dialog.ShowDialog() == true
                             ? dialog.Result
                             : new TransferErrorDialogResult(CopyBehavior.Cancel, CopyBehaviorScope.All);

            switch (result.Behavior)
            {
                case CopyBehavior.Overwrite:
                case CopyBehavior.Resume:
                    retry.Invoke(result.Behavior);
                    break;
                case CopyBehavior.Skip:
                    skip.Invoke(false);
                    break;
                case CopyBehavior.Cancel:
                    cancel.Invoke();
                    break;
            }

            switch (result.Scope)
            {
                case CopyBehaviorScope.All:
                    _copyBehavior = result.Behavior;
                    break;
                case CopyBehaviorScope.AllOlder:
                    _overwriteOlder = true;
                    break;
            }
        }

        private void UpdateTransfer(bool result)
        {
            var item = _queue.Dequeue();
            FilesTransfered++;
            if (item.Type == ItemType.File) BytesTransfered += item.Size ?? 0;
            if (result) item.IsSelected = false;
        }

        private void FinishTransfer()
        {
            IsInProgress = false;
            eventAggregator.GetEvent<TransferFinishedEvent>().Publish(new TransferFinishedEventArgs());
        }
    }
}