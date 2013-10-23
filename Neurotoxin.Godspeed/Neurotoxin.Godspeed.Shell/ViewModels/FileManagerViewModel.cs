﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class FileManagerViewModel : ViewModelBase
    {
        private Queue<QueueItem> _queue;
        private CopyAction _rememberedCopyAction;
        private const string RenameFromPattern = @"([\/]){0}$";
        private const string RenameToPattern = @"$1{0}";

        #region Main window properties

        private readonly Stack<IPaneViewModel> _leftPaneStack = new Stack<IPaneViewModel>();

        private const string LEFTPANE = "LeftPane";
        private IPaneViewModel _leftPane;
        public IPaneViewModel LeftPane
        {
            get { return _leftPane; }
            set { _leftPane = value; NotifyPropertyChanged(LEFTPANE); }
        }

        private readonly Stack<IPaneViewModel> _rightPaneStack = new Stack<IPaneViewModel>();

        private const string RIGHTPANE = "RightPane";
        private IPaneViewModel _rightPane;
        public IPaneViewModel RightPane
        {
            get { return _rightPane; }
            set { _rightPane = value; NotifyPropertyChanged(RIGHTPANE); }
        }

        private IPaneViewModel ActivePane
        {
            get 
            { 
                if (LeftPane != null && LeftPane.IsActive) return LeftPane;
                if (RightPane != null && RightPane.IsActive) return RightPane;
                return null;
            }
        }

        private IPaneViewModel OtherPane
        {
            get { return LeftPane.IsActive ? RightPane : LeftPane; }
        }

        private IFileListPaneViewModel SourcePane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && left.IsActive) return left;
                if (right != null && right.IsActive) return right;
                return null;
            }
        }

        private IFileListPaneViewModel TargetPane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && !left.IsActive) return left;
                if (right != null && !right.IsActive) return right;
                return null;
            }
        }

        #endregion

        #region Transfer properties

        private const string TRANSFERTYPE = "TransferType";
        private TransferType _transferType;
        public TransferType TransferType
        {
            get { return _transferType; }
            set { _transferType = value; NotifyPropertyChanged(TRANSFERTYPE); }
        }

        private const string SOURCEFILE = "SourceFile";
        private string _sourceFile;
        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; NotifyPropertyChanged(SOURCEFILE); }
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
            get
            {
                switch (TransferType)
                {
                    case TransferType.Delete:
                        return FilesTransfered*100/FileCount;
                    default:
                        return (int)(BytesTransfered * 100 / TotalBytes);
                }
            }
        }

        private const string FILESTRANSFERED = "FilesTransfered";
        private int _filesTransfered;
        public int FilesTransfered
        {
            get { return _filesTransfered; }
            set { _filesTransfered = value; NotifyPropertyChanged(FILESTRANSFERED); NotifyPropertyChanged(TOTALPROGRESS); }
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
            set { _bytesTransfered = value; NotifyPropertyChanged(BYTESTRANSFERED); }
        }

        private const string TOTALBYTES = "TotalBytes";
        private long _totalBytes;
        public long TotalBytes
        {
            get { return _totalBytes; }
            set { _totalBytes = value; NotifyPropertyChanged(TOTALBYTES); }
        }

        #endregion

        #region SwitchPaneCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SwitchPaneCommand { get; private set; }

        private bool CanExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            return eventInformation.EventArgs.Key == Key.Tab;
        }

        private void ExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            OtherPane.SetActive();
            eventInformation.EventArgs.Handled = true;
        }

        #endregion

        #region EditCommand

        public DelegateCommand EditCommand { get; private set; }

        private bool CanExecuteEditCommand()
        {
            var pane = ActivePane as ConnectionsViewModel;
            return pane != null && pane.SelectedItem is FtpConnectionItemViewModel && !pane.IsBusy;
        }
        
        private void ExecuteEditCommand()
        {
            ((ConnectionsViewModel)ActivePane).Edit();
        }

        #endregion

        #region CopyCommand

        public DelegateCommand CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand()
        {
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection() && !SourcePane.IsBusy && !TargetPane.IsBusy;
        }

        private void ExecuteCopyCommand()
        {
            var message = string.Format("Do you really want to copy the selected items?");
            if (new ConfirmationDialog("Copy", message).ShowDialog() != true) return;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Copy), CopyPrepare, CopyError);
        }

        private void CopyPrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Copy);
            CopyStart();
        }

        private void CopyStart(CopyAction? action = null, string rename = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek().FileSystemItem;
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() => CopyInner(item, action, rename), CopySuccess, CopyError);
            }
            else
            {
                CopyFinish();
                _queue = null;
            }            
        }

        private bool CopyInner(FileSystemItem item, CopyAction? action, string rename)
        {
            var sourcePath = item.GetRelativePath(SourcePane.CurrentFolder.Path);
            var targetPath = TargetPane.GetTargetPath(sourcePath);

            switch (item.Type)
            {
                case ItemType.Directory:
                    TargetPane.CreateFolder(targetPath);
                    return true;
                case ItemType.File:
                    if (action == CopyAction.Rename && !string.IsNullOrEmpty(rename))
                    {
                        var r = new Regex(string.Format(RenameFromPattern, item.Name), RegexOptions.IgnoreCase);
                        targetPath = r.Replace(targetPath, string.Format(RenameToPattern, rename));
                        action = CopyAction.CreateNew;
                    }

                    var a = action ?? _rememberedCopyAction;

                    if (TargetPane is LocalFileSystemContentViewModel) return SourcePane.Export(item, targetPath, a);
                    if (SourcePane is LocalFileSystemContentViewModel) return TargetPane.Import(item, targetPath, a);
                    var tempFile = string.Format("{0}\\temp\\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), item.FullPath.Hash());
                    var tempItem = item.Clone();
                    tempItem.Path = tempFile;
                    var result = SourcePane.Export(item, tempFile, CopyAction.Overwrite) &&
                                 TargetPane.Import(tempItem, targetPath, action ?? _rememberedCopyAction);
                    File.Delete(tempFile);
                    return result;
                default:
                    throw new NotSupportedException();
            }
        }

        private void CopySuccess(bool result)
        {
            var item = _queue.Dequeue().FileSystemItem;
            if (item.Type == ItemType.File) BytesTransfered += item.Size ?? 0;
            FilesTransfered++;
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path.StartsWith(i.Path));
            if (vm != null && !_queue.Any(q => q.FileSystemItem.Path.StartsWith(vm.Path)))
                vm.IsSelected = false;
            CopyStart();
        }

        private void CopyError(Exception exception)
        {
            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    CopyStart(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, CopyStart, CopyError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    CopySuccess(false);
                    break;
                case ErrorResolutionBehavior.Cancel:
                    CopyFinish();
                    break;
            }
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
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection() && !SourcePane.IsBusy && !TargetPane.IsBusy;
        }

        private void ExecuteMoveCommand()
        {
            var message = string.Format("Do you really want to move the selected items?");
            if (new ConfirmationDialog("Move", message).ShowDialog() != true) return;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Move), MovePrepare, MoveError);
        }

        private void MovePrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Move);
            MoveStart();
        }

        private void MoveStart(CopyAction? action = null, string rename = null)
        {
            if (_queue.Count > 0)
            {
                var queueitem = _queue.Peek();
                var item = queueitem.FileSystemItem;
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() =>
                    {
                        switch (queueitem.TransferType)
                        {
                            case TransferType.Copy:
                                CopyInner(item, action, rename);
                                break;
                            case TransferType.Delete:
                                SourcePane.Delete(item);
                                break;
                            default:
                                throw new NotSupportedException("Invalid transfer type: " + queueitem.TransferType);
                        }
                        return true;
                    }, MoveSuccess, MoveError);
            }
            else
            {
                MoveFinish();
                _queue = null;
            }
        }

        private void MoveSuccess(bool result)
        {
            var queueitem = _queue.Dequeue();
            var item = queueitem.FileSystemItem;
            if (queueitem.TransferType == TransferType.Copy)
            {
                if (item.Type == ItemType.File) BytesTransfered += item.Size ?? 0;
                FilesTransfered++;
            }
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path == i.Path);
            if (vm != null) vm.IsSelected = false;
            MoveStart();
        }

        private void MoveError(Exception exception)
        {
            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    MoveStart(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, MoveStart, MoveError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    MoveSuccess(false);
                    break;
                case ErrorResolutionBehavior.Cancel:
                    MoveFinish();
                    break;
            }
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
            return SourcePane != null && !SourcePane.IsBusy;
        }

        private void ExecuteNewFolderCommand()
        {
            var dialog = new InputDialog("Add New Folder", "Folder name:", string.Empty);
            if (dialog.ShowDialog() != true) return;
            var name = dialog.Input.Text;
            var path = string.Format("{0}{1}", SourcePane.CurrentFolder.Path, name);
            WorkerThread.Run(() => SourcePane.CreateFolder(path), success => NewFolderSuccess(success, name), NewFolderError);
        }

        private void NewFolderSuccess(bool success, string name)
        {
            if (!success)
            {
                NotificationMessage.Show("Add New Folder", string.Format("Error: folder [{0}] already exists! Please specify a different name.", name));
                return;
            }
            SourcePane.Refresh(() =>
                                   {
                                       SourcePane.CurrentRow = SourcePane.Items.Single(item => item.Name == name);
                                   });
        }

        private void NewFolderError(Exception ex)
        {
            ShowCorrespondingErrorDialog(ex);
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand DeleteCommand { get; private set; }

        private bool CanExecuteDeleteCommand()
        {
            return SourcePane != null && SourcePane.HasValidSelection() && !SourcePane.IsBusy;
        }

        private void ExecuteDeleteCommand()
        {
            var message = string.Format("Do you really want to delete the selected items?");
            if (new ConfirmationDialog("Delete", message).ShowDialog() != true) return;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Delete), DeletePrepare, DeleteError);
        }

        private void DeletePrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Delete);
            DeleteStart();
        }

        private void DeleteStart()
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek().FileSystemItem;
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() => SourcePane.Delete(item), DeleteSuccess, DeleteError);
            }
            else
            {
                DeleteFinish();
                _queue = null;
            }
        }

        private void DeleteSuccess(bool result)
        {
            var item = _queue.Dequeue().FileSystemItem;
            if (item.Type == ItemType.File) BytesTransfered += item.Size ?? 0;
            FilesTransfered++;
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path == i.Path);
            if (vm != null) vm.IsSelected = false;
            DeleteStart();
        }

        private void DeleteError(Exception exception)
        {
            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    DeleteStart();
                    break;
                case ErrorResolutionBehavior.Skip:
                    DeleteSuccess(false);
                    break;
                case ErrorResolutionBehavior.Cancel:
                    DeleteFinish();
                    break;
            }
        }

        private void DeleteFinish()
        {
            FinishTransfer();
            SourcePane.Refresh();
        }

        #endregion

        #region Events

        public event TransferStartedEventHandler TransferStarted;

        private void NotifyTransferStarted()
        {
            var handler = TransferStarted;
            if (handler != null) handler.Invoke();
        }

        public event TransferFinishedEventHandler TransferFinished;

        private void NotifyTransferFinished()
        {
            var handler = TransferFinished;
            if (handler != null) handler.Invoke();
        }

        #endregion

        public FileManagerViewModel()
        {
            SwitchPaneCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSwitchPaneCommand, CanExecuteSwitchPaneCommand);
            EditCommand = new DelegateCommand(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);

            eventAggregator.GetEvent<FtpOperationProgressChangedEvent>().Subscribe(OnFtpOperationProgressChanged);
            eventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnOpenNestedPane);
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Subscribe(OnCloseNestedPane);
        }

        public void InitializePanes()
        {
            LeftPane = container.Resolve<LocalFileSystemContentViewModel>();
            LeftPane.LoadDataAsync(LoadCommand.Load, null);
            RightPane = container.Resolve<ConnectionsViewModel>();
            RightPane.LoadDataAsync(LoadCommand.Load, null);
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

        public void FtpDisconnect()
        {
            RightPane = container.Resolve<ConnectionsViewModel>();
        }

        private void OnFtpOperationProgressChanged(FtpOperationProgressChangedEventArgs args)
        {
            UIThread.Run(() => CurrentFileProgress = args.Percentage > 0 ? args.Percentage : 0);
        }

        private void OnOpenNestedPane(OpenNestedPaneEventArgs args)
        {
            if (LeftPane == args.Opener)
            {
                _leftPaneStack.Push(LeftPane);
                LeftPane = args.Openee;
            }
            else if (RightPane == args.Opener)
            {
                _rightPaneStack.Push(RightPane);
                RightPane = args.Openee;
            }
        }

        private void OnCloseNestedPane(CloseNestedPaneEventArgs args)
        {
            if (LeftPane == args.Pane)
            {
                LeftPane = _leftPaneStack.Pop();
                LeftPane.LoadDataAsync(LoadCommand.Restore, args.Payload);
                
            }
            else if (RightPane == args.Pane)
            {
                RightPane = _rightPaneStack.Pop();
                RightPane.LoadDataAsync(LoadCommand.Restore, args.Payload);
            }
        }

        private void InitializeTransfer(TransferType mode)
        {
            TransferType = mode;
            _rememberedCopyAction = CopyAction.CreateNew;
            FileCount = _queue.Count;
            if (mode == TransferType.Move) FileCount /= 2;
            TotalBytes = _queue.Where(item => item.FileSystemItem.Type == ItemType.File).Sum(item => item.FileSystemItem.Size ?? 0);
            FilesTransfered = 0;
            BytesTransfered = 0;
            NotifyTransferStarted();
        }

        internal TransferErrorDialogResult ShowCorrespondingErrorDialog(Exception exception)
        {
            var transferException = exception as TransferException;
            var result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
            if (transferException != null)
            {
                switch (transferException.Type)
                {
                    case TransferErrorType.ReadAccessError:
                        {
                            var dialog = new ReadErrorDialog(exception);
                            if (dialog.ShowDialog() == true) result = dialog.Result;
                        }
                        break;
                    case TransferErrorType.WriteAccessError:
                        {
                            var dialog = new WriteErrorDialog(transferException, eventAggregator);
                            SourcePane.GetItemViewModel(transferException.SourceFile);
                            TargetPane.GetItemViewModel(transferException.TargetFile);
                            if (dialog.ShowDialog() == true) result = dialog.Result;
                        }
                        break;
                    case TransferErrorType.LostConnection:
                        result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
                        var reconnectionDialog = new ReconnectionDialog(exception);
                        if (reconnectionDialog.ShowDialog() == true)
                        {
                            var ftp = LeftPane as FtpContentViewModel ?? RightPane as FtpContentViewModel;
                            ftp.RestoreConnection();
                        }
                        else
                        {
                            FtpDisconnect();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Invalid transfer error type: " + transferException.Type);
                }

                if (result.Scope == CopyActionScope.All && result.Action.HasValue)
                {
                    _rememberedCopyAction = result.Action.Value;
                }
            }
            else
            {
                NotificationMessage.Show("Unknown error occured", exception.Message);
            }
            return result;
        }

        internal void AbortTransfer()
        {
            var ftp = LeftPane as FtpContentViewModel ?? RightPane as FtpContentViewModel;
            if (ftp != null)
            {
                ftp.Abort();
            } 
            else
            {
                lock (_queue)
                {
                    var actualItem = _queue.Peek();
                    _queue.Clear();
                    _queue.Enqueue(actualItem);
                }
            }
        }

        private void FinishTransfer()
        {
            NotifyTransferFinished();
        }

        private void RenameExistingFile(TransferException exception, CopyAction? action, Action<CopyAction?, string> rename, Action<Exception> chooseDifferentOption)
        {
            var dialog = new InputDialog("Rename", "New name:", Path.GetFileName(exception.TargetFile));
            if (dialog.ShowDialog() == true)
            {
                rename.Invoke(action, dialog.Input.Text);
            }
            else
            {
                chooseDifferentOption.Invoke(exception);
            }
        }

        private void AsyncJob<T>(Func<T> work, Action<T> success, Action<Exception> error = null)
        {
            var finished = false;
            NotificationMessage notificationMessage = null;
            WorkerThread.Run(
                () =>
                    {
                        Thread.Sleep(3000); 
                        return true;
                    }, 
                b =>
                    {
                        if (finished) return;
                        notificationMessage = new NotificationMessage("Application is busy", "Populating. Please wait...", false);
                        notificationMessage.ShowDialog();
                    });
            WorkerThread.Run(work, 
                b =>
                    {
                        if (notificationMessage != null)
                        {
                            notificationMessage.Close();
                            notificationMessage = null;
                        }
                        finished = true;
                        success.Invoke(b);
                    }, 
                e =>
                    {
                        if (notificationMessage != null)
                        {
                            notificationMessage.Close();
                            notificationMessage = null;
                        }
                        finished = true;
                        if (error != null) error.Invoke(e);
                    });
        }
    }
}