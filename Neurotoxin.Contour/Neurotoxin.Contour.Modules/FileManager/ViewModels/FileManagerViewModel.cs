using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Events;
using Neurotoxin.Contour.Modules.FileManager.Exceptions;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Modules.FileManager.Views.Dialogs;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{

    public class FileManagerViewModel : ModuleViewModelBase
    {
        private readonly IUnityContainer _container;
        private Queue<FileSystemItemViewModel> _queue;
        private CopyAction _rememberedCopyAction;

        #region Main window properties

        private const string LEFTPANE = "LeftPane";
        private IPaneViewModel _leftPane;
        public IPaneViewModel LeftPane
        {
            get { return _leftPane; }
            set { _leftPane = value; NotifyPropertyChanged(LEFTPANE); }
        }

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
                if (LeftPane.IsActive) return LeftPane;
                if (RightPane.IsActive) return RightPane;
                return null;
            }
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

        private const string TRANSFERPROGRESSDIALOGMODE = "TransferProgressDialogMode";
        private TransferProgressDialogMode _transferProgressDialogMode;
        public TransferProgressDialogMode TransferProgressDialogMode
        {
            get { return _transferProgressDialogMode; }
            set { _transferProgressDialogMode = value; NotifyPropertyChanged(TRANSFERPROGRESSDIALOGMODE); }
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
                switch (TransferProgressDialogMode)
                {
                    case TransferProgressDialogMode.Delete:
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
                    LeftPane = _container.Resolve<LocalFileSystemContentViewModel>();
                    LeftPane.LoadDataAsync(cmd, cmdParam);
                    RightPane = _container.Resolve<ConnectionsViewModel>();
                    RightPane.LoadDataAsync(cmd, cmdParam);
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
            var pane = ActivePane as ConnectionsViewModel;
            return pane != null && pane.SelectedItem is FtpConnectionItemViewModel;
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
            return TargetPane != null && SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteCopyCommand()
        {
            WorkerThread.Run(SourcePane.PopulateQueue, CopyPrepare);
        }

        private void CopyPrepare(Queue<FileSystemItemViewModel> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferProgressDialogMode.Copy);
            CopyStart();
        }

        private void CopyStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() => CopyInner(item, action), CopySuccess, CopyError);
            }
            else
            {
                CopyFinish();
                _queue = null;
            }            
        }

        private bool CopyInner(FileSystemItemViewModel item, CopyAction? action)
        {
            var sourcePane = SourcePane as FtpContentViewModel;
            var targetPane = TargetPane as FtpContentViewModel;
            return sourcePane != null
                ? sourcePane.Download(item, TargetPane.CurrentFolder.Path, action ?? _rememberedCopyAction)
                : targetPane.Upload(item, SourcePane.CurrentFolder.Path, action ?? _rememberedCopyAction);
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
            return TargetPane != null && SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteMoveCommand()
        {
            WorkerThread.Run(SourcePane.PopulateQueue, MovePrepare);
        }

        private void MovePrepare(Queue<FileSystemItemViewModel> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferProgressDialogMode.Move);
            MoveStart();
        }

        private void MoveStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() =>
                    {
                        CopyInner(item, action);
                        SourcePane.Delete(item); 
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
            InitializeTransfer(TransferProgressDialogMode.Delete);
            DeleteStart();
        }

        private void DeleteStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
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
            UpdateTransfer(result);
            var item = _queue.Dequeue();
            if (result) item.IsSelected = false;
            DeleteStart();
        }

        private void DeleteError(Exception exception)
        {
            ShowTransferErrorDialog(exception, DeleteStart, DeleteSuccess, DeleteFinish);
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

        public FileManagerViewModel(IUnityContainer container)
        {
            _container = container;
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

        internal void FtpConnect(IStoredConnectionViewModel connection)
        {
            var ftp = _container.Resolve<FtpContentViewModel>();
            ftp.LoadDataAsync(LoadCommand.Load, connection, FtpConnectSuccess, FtpConnectError);
        }

        private void FtpConnectSuccess(PaneViewModelBase viewModel)
        {
            var ftp = (FtpContentViewModel) viewModel;
            ftp.FileManager.FtpOperationStarted += FtpWrapperOnFtpOperationStarted;
            ftp.FileManager.FtpOperationFinished += FtpWrapperOnFtpOperationFinished;
            ftp.FileManager.FtpOperationProgressChanged += FtpWrapperOnFtpOperationProgressChanged;
            RightPane = ftp;
        }

        private void FtpConnectError(PaneViewModelBase viewModel)
        {
            //TODO
        }

        public void FtpDisconnect()
        {
            RightPane = _container.Resolve<ConnectionsViewModel>();            
        }

        private void InitializeTransfer(TransferProgressDialogMode mode)
        {
            _rememberedCopyAction = CopyAction.CreateNew;
            FileCount = _queue.Count;
            TotalBytes = _queue.Where(item => item.Type == ItemType.File).Sum(item => item.Size ?? 0);
            FilesTransfered = 0;
            BytesTransfered = 0;
            IsInProgress = true;
            TransferProgressDialogMode = mode;
            NotifyTransferStarted();
        }

        private void ShowTransferErrorDialog(Exception exception, Action<CopyAction?> retry, Action<bool> skip, Action cancel)
        {
            var transferException = exception as TransferException;
            if (transferException != null)
            {
                TransferErrorDialogResult result;
                switch (transferException.Type)
                {
                    case TransferErrorType.ReadAccessError:
                        {
                            var dialog = new ReadErrorDialog(exception);
                            result = dialog.ShowDialog() == true
                                         ? dialog.Result
                                         : new TransferErrorDialogResult(CopyBehavior.Cancel);
                        }
                        break;
                    case TransferErrorType.WriteAccessError:
                        {
                            var dialog = new WriteErrorDialog(transferException, SourcePane.GetItemViewModel(transferException.SourceFile), TargetPane.GetItemViewModel(transferException.TargetFile));
                            result = dialog.ShowDialog() == true
                                         ? dialog.Result
                                         : new TransferErrorDialogResult(CopyBehavior.Cancel);
                        }
                        break;
                    case TransferErrorType.LostConnection:
                        result = new TransferErrorDialogResult(CopyBehavior.Cancel);
                        var reconnectionDialog = new ReconnectionDialog();
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

                switch (result.Behavior)
                {
                    case CopyBehavior.Retry:
                        retry.Invoke(result.Action);
                        break;
                    case CopyBehavior.Skip:
                        skip.Invoke(false);
                        break;
                    case CopyBehavior.Cancel:
                        cancel.Invoke();
                        break;
                }
            }
            else
            {
                MessageBox.Show("Uknown error occured: " + exception.Message);
            }
        }

        private void UpdateTransfer(bool result)
        {
            var item = _queue.Dequeue();
            FilesTransfered++;
            if (item.Type == ItemType.File) BytesTransfered += item.Size ?? 0;
            if (result) item.IsSelected = false;
        }

        internal void AbortTransfer()
        {
            throw new NotImplementedException();
        }

        private void FinishTransfer()
        {
            IsInProgress = false;
            NotifyTransferFinished();
        }

        public void OpenStfsPackage(FileSystemItemViewModel item)
        {
            WorkerThread.Run(() => SourcePane.ReadFileContent(item.Path), OpenStfsPackageCallback);
        }

        private void OpenStfsPackageCallback(byte[] content)
        {
            RightPane = _container.Resolve<StfsPackageContentViewModel>();
            RightPane.LoadDataAsync(LoadCommand.Load, content);
        }
    }
}