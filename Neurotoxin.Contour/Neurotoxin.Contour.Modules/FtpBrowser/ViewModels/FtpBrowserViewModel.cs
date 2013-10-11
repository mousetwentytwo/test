using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Exceptions;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.Views;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{

    public class FtpBrowserViewModel : ModuleViewModelBase
    {
        private Queue<FileSystemItemViewModel> _queue;
        private CopyAction _rememberedCopyAction;

        #region Main window properties

        private const string STOREDCONNECTIONS = "StoredConnections";
        private StoredConnectionsViewModel _storedConnections;
        public StoredConnectionsViewModel StoredConnections
        {
            get { return _storedConnections; }
            set { _storedConnections = value; NotifyPropertyChanged(STOREDCONNECTIONS); }
        }

        private const string STOREDCONNECTIONSVISIBILITY = "StoredConnectionsVisibility";
        public Visibility StoredConnectionsVisibility
        {
            get { return Ftp != null ? Visibility.Collapsed : Visibility.Visible; }
        }

        private const string FTP = "Ftp";
        private FtpContentViewModel _ftp;
        public FtpContentViewModel Ftp
        {
            get { return _ftp; }
            set { _ftp = value; NotifyPropertyChanged(FTP); }
        }

        private const string FTPVISIBILITY = "FtpVisibility";
        public Visibility FtpVisibility
        {
            get { return Ftp != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        private const string LOCALFILESYSTEM = "LocalFileSystem";
        private LocalPaneViewModel _localFileSystem;
        public LocalPaneViewModel LocalFileSystem
        {
            get { return _localFileSystem; }
            set { _localFileSystem = value; NotifyPropertyChanged(LOCALFILESYSTEM); }
        }

        private IPaneViewModel RightPane
        {
            get { return Ftp != null ? (IPaneViewModel)Ftp : StoredConnections; }
        }

        private IFileListPaneViewModel SourcePane
        {
            get
            {
                var rightPane = RightPane as IFileListPaneViewModel;
                return rightPane != null && rightPane.IsActive ? rightPane : (LocalFileSystem.IsActive ? LocalFileSystem : null);
            }
        }

        private IFileListPaneViewModel TargetPane
        {
            get
            {
                var rightPane = RightPane as IFileListPaneViewModel;
                return rightPane != null && rightPane.IsActive ? LocalFileSystem : (LocalFileSystem.IsActive ? rightPane : null);
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
                    StoredConnections = new StoredConnectionsViewModel(this);
                    StoredConnections.LoadDataAsync(cmd, cmdParam);
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
            return false;
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
            return TargetPane != null && SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteCopyCommand()
        {
            _queue = SourcePane.PopulateQueue();
            InitializeTransfer(TransferProgressDialogMode.Copy);
            CopyStart();
        }

        private void CopyStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                SourceFile = item.Path.Replace(Ftp.CurrentFolder.Path, string.Empty);
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => Ftp.Download(item, TargetPane.CurrentFolder.Path, action ?? _rememberedCopyAction), CopySuccess, CopyError);
                }
                else
                {
                    WorkerThread.Run(() => Ftp.Upload(item, SourcePane.CurrentFolder.Path, action ?? _rememberedCopyAction), CopySuccess, CopyError);
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
            return TargetPane != null && SourcePane != null && SourcePane.CurrentRow != null;
        }

        private void ExecuteMoveCommand()
        {
            _queue = SourcePane.PopulateQueue();
            InitializeTransfer(TransferProgressDialogMode.Move);
            MoveStart();
        }

        private void MoveStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                SourceFile = item.Path.Replace(Ftp.CurrentFolder.Path, string.Empty);
                if (SourcePane == Ftp)
                {
                    WorkerThread.Run(() => { Ftp.Download(item, TargetPane.CurrentFolder.Path, action ?? _rememberedCopyAction); Ftp.Delete(item); return true; }, MoveSuccess, MoveError);
                }
                else
                {
                    WorkerThread.Run(() => { Ftp.Upload(item, SourcePane.CurrentFolder.Path, action ?? _rememberedCopyAction); LocalFileSystem.Delete(item); return true; }, MoveSuccess, MoveError);
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
            InitializeTransfer(TransferProgressDialogMode.Delete);
            DeleteStart();
        }

        private void DeleteStart(CopyAction? action = null)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                SourceFile = item.Path.Replace(Ftp.CurrentFolder.Path, string.Empty);
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

        internal void FtpConnect(IStoredConnectionViewModel connection)
        {
            Ftp = new FtpContentViewModel(this);
            Ftp.LoadDataAsync(LoadCommand.Load, connection, FtpConnectSuccess, FtpConnectError);
        }

        private void FtpConnectSuccess()
        {
            Ftp.FileManager.FtpOperationStarted += FtpWrapperOnFtpOperationStarted;
            Ftp.FileManager.FtpOperationFinished += FtpWrapperOnFtpOperationFinished;
            Ftp.FileManager.FtpOperationProgressChanged += FtpWrapperOnFtpOperationProgressChanged;
            NotifyPropertyChanged(FTPVISIBILITY);
            NotifyPropertyChanged(STOREDCONNECTIONSVISIBILITY);
        }

        private void FtpConnectError()
        {
            FtpDisconnect();
        }

        internal void FtpDisconnect()
        {
            Ftp = null;
            NotifyPropertyChanged(FTPVISIBILITY);
            NotifyPropertyChanged(STOREDCONNECTIONSVISIBILITY);
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
                        result = ShowTransferErrorDialog<ReadErrorDialog>(exception);
                        break;
                    case TransferErrorType.WriteAccessError:
                        result = ShowTransferErrorDialog<WriteErrorDialog>(exception);
                        break;
                    case TransferErrorType.LostConnection:
                        result = new TransferErrorDialogResult(CopyBehavior.Cancel);
                        var reconnectionDialog = new ReconnectionDialog();
                        if (reconnectionDialog.ShowDialog() == true)
                        {
                            Ftp.RestoreConnection();
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

        private TransferErrorDialogResult ShowTransferErrorDialog<T>(Exception exception) where T : ITransferErrorDialog
        {
            var dialogConstructor = typeof (T).GetConstructor(new[] {typeof (Exception)});
            if (dialogConstructor == null) throw new Exception("Constructor is missing");
            var dialog = (T)dialogConstructor.Invoke(new object[] { exception});
            return dialog.ShowDialog() == true
                             ? dialog.Result
                             : new TransferErrorDialogResult(CopyBehavior.Cancel);
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
    }
}