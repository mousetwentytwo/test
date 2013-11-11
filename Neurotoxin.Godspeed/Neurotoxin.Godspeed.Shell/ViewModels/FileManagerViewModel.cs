using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Exceptions;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
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
        private CopyMode _copyMode;
        private CopyAction _rememberedCopyAction;
        private const string RenameFromPattern = @"([\/]){0}$";
        private const string RenameToPattern = @"$1{0}";
        private readonly Stopwatch _speedMeter = new Stopwatch();
        private readonly Stopwatch _elapsedTimeMeter = new Stopwatch();
        private bool _isAborted;

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
                        return FilesTransferred*100/FileCount;
                    default:
                        return (int)(BytesTransferred * 100 / TotalBytes);
                }
            }
        }

        private const string TOTALPROGRESSDOUBLE = "TotalProgressDouble";
        public double TotalProgressDouble
        {
            get { return (double) TotalProgress/100; }
        }

        private const string FILESTRANSFERRED = "FilesTransferred";
        private int _filesTransferred;
        public int FilesTransferred
        {
            get { return _filesTransferred; }
            set
            {
                _filesTransferred = value; 
                NotifyPropertyChanged(FILESTRANSFERRED); 
                NotifyPropertyChanged(TOTALPROGRESS);
                NotifyPropertyChanged(TOTALPROGRESSDOUBLE);
            }
        }

        private const string FILECOUNT = "FileCount";
        private int _fileCount;
        public int FileCount
        {
            get { return _fileCount; }
            set { _fileCount = value; NotifyPropertyChanged(FILECOUNT); }
        }

        private const string BYTESTRANSFERRED = "BytesTransferred";
        private long _bytesTransferred;
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
            set
            {
                _bytesTransferred = value;
                NotifyPropertyChanged(BYTESTRANSFERRED);
                NotifyPropertyChanged(TOTALPROGRESS);
                NotifyPropertyChanged(TOTALPROGRESSDOUBLE);
            }
        }

        private const string TOTALBYTES = "TotalBytes";
        private long _totalBytes;
        public long TotalBytes
        {
            get { return _totalBytes; }
            set { _totalBytes = value; NotifyPropertyChanged(TOTALBYTES); }
        }

        private const string SPEED = "Speed";
        private int _speed;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; NotifyPropertyChanged(SPEED); }
        }

        private const string ELAPSEDTIME = "ElapsedTime";
        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; NotifyPropertyChanged(ELAPSEDTIME); }
        }

        private const string REMAININGTIME = "RemainingTime";
        private TimeSpan _remainingTime;
        public TimeSpan RemainingTime
        {
            get { return _remainingTime; }
            set { _remainingTime = value; NotifyPropertyChanged(REMAININGTIME); }
        }

        private const string PROGRESSSTATE = "ProgressState";
        private TaskbarItemProgressState _progressState;
        public TaskbarItemProgressState ProgressState
        {
            get { return _progressState; }
            set { _progressState = value; NotifyPropertyChanged(PROGRESSSTATE); }
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
            var message = string.Format("Do you really want to copy the selected item(s)?");
            if (new ConfirmationDialog("Copy", message).ShowDialog() != true) return;
            _isAborted = false;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Copy), CopyPrepare, CopyError);
        }

        private void CopyPrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Copy, () => CopyStart());
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

                    switch (_copyMode)
                    {
                        case CopyMode.DirectExport:
                            return SourcePane.Export(item, targetPath, a);
                        case CopyMode.DirectImport:
                            return TargetPane.Import(item, targetPath, a);
                        case CopyMode.Indirect:
                            var tempFile = string.Format("{0}\\temp\\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), item.FullPath.Hash());
                            var tempItem = item.Clone();
                            tempItem.Path = tempFile;
                            var result = SourcePane.Export(item, tempFile, CopyAction.Overwrite) &&
                                         TargetPane.Import(tempItem, targetPath, action ?? _rememberedCopyAction);
                            File.Delete(tempFile);
                            return result;
                        case CopyMode.RemoteExport:
                            return ((FtpContentViewModel)TargetPane).RemoteUpload(item, targetPath, a);
                        case CopyMode.RemoteImport:
                            return ((FtpContentViewModel)SourcePane).RemoteDownload(item, targetPath, a);
                        default:
                            throw new NotSupportedException("Invalid Copy Mode: " + _copyMode);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private void CopySuccess(bool result)
        {
            var item = _queue.Dequeue().FileSystemItem;
            FilesTransferred++;
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path.StartsWith(i.Path));
            if (vm != null && !_queue.Any(q => q.FileSystemItem.Path.StartsWith(vm.Path)))
                vm.IsSelected = false;
            CopyStart();
        }

        private void CopyError(Exception exception)
        {
            if (_isAborted) return;
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

        private void OpenTelnetSession(LocalFileSystemContentViewModel local, FtpContentViewModel ftp)
        {
            var connection = ftp.Connection;
            Telnet.OpenSession(local.Drive.FullPath, connection.Address, connection.Port ?? 21, connection.Username, connection.Password);
        }

        private void CloseTelnetSession()
        {
            Telnet.CloseSession();
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
            var message = string.Format("Do you really want to move the selected item(s)?");
            if (new ConfirmationDialog("Move", message).ShowDialog() != true) return;
            _isAborted = false;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Move), MovePrepare, MoveError);
        }

        private void MovePrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Move, () => MoveStart());
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
                //if (item.Type == ItemType.File) BytesTransferred += item.Size ?? 0;
                FilesTransferred++;
            }
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path == i.Path);
            if (vm != null) vm.IsSelected = false;
            MoveStart();
        }

        private void MoveError(Exception exception)
        {
            if (_isAborted) return;
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
            var items = SourcePane.Items.Select(item => item.Name).ToList();
            var wkDirs = DirectoryStructure.WellKnownDirectoriesOf(SourcePane.CurrentFolder.Path);
            var suggestion = wkDirs.Where(d => !items.Contains(d)).Select(d => new InputDialogOptionViewModel
                {
                    Value = d,
                    DisplayName = TitleRecognizer.GetTitle(d)
                });

            var name = InputDialog.Show("Add New Folder", "Folder name:", string.Empty, suggestion);
            if (string.IsNullOrEmpty(name)) return;
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
            var connections = ActivePane as ConnectionsViewModel;
            if (connections != null && connections.SelectedItem != null) return true;
            return SourcePane != null && SourcePane.HasValidSelection() && !SourcePane.IsBusy;
        }

        private void ExecuteDeleteCommand()
        {
            var connections = ActivePane as ConnectionsViewModel;
            var s = connections != null ? string.Empty : "(s)";

            var message = string.Format("Do you really want to delete the selected item{0}?", s);
            if (new ConfirmationDialog("Delete", message).ShowDialog() != true) return;

            if (connections != null)
            {
                connections.Delete();
            }
            else
            {
                _isAborted = false;
                AsyncJob(() => SourcePane.PopulateQueue(TransferType.Delete), DeletePrepare, DeleteError);
            }
        }

        private void DeletePrepare(Queue<QueueItem> queue)
        {
            _queue = queue;
            InitializeTransfer(TransferType.Delete, DeleteStart);
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
            if (item.Type == ItemType.File) BytesTransferred += item.Size ?? 0;
            FilesTransferred++;
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path == i.Path);
            if (vm != null) vm.IsSelected = false;
            DeleteStart();
        }

        private void DeleteError(Exception exception)
        {
            if (_isAborted) return;
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

            eventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
            eventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnOpenNestedPane);
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Subscribe(OnCloseNestedPane);
        }

        public void InitializePanes()
        {
            LeftPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(UserSettings.LeftPaneType, typeof(LocalFileSystemContentViewModel)));
            var leftParam = UserSettings.Get(UserSettings.LeftPaneFileListPaneSettings, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending));
            LeftPane.LoadDataAsync(LoadCommand.Load, leftParam);

            RightPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(UserSettings.RightPaneType, typeof(ConnectionsViewModel)));
            var rightParam = UserSettings.Get(UserSettings.RightPaneFileListPaneSettings, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending));
            RightPane.LoadDataAsync(LoadCommand.Load, rightParam);
        }

        private static Type GetStoredPaneType(string key, Type defaultValue)
        {
            var asm = Assembly.GetExecutingAssembly();
            var typeName = UserSettings.Get<string>(key);
            return !string.IsNullOrEmpty(typeName) ? asm.GetType(typeName) : defaultValue;
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

        private void FtpDisconnect()
        {
            RightPane = container.Resolve<ConnectionsViewModel>();
        }

        private void OnTransferProgressChanged(TransferProgressChangedEventArgs args)
        {
            UIThread.Run(() =>
                             {
                                 var elapsed = _elapsedTimeMeter.Elapsed;
                                 var transferred = BytesTransferred;
                                 ElapsedTime = elapsed;
                                 if (elapsed.Ticks > 0 && transferred > 0)
                                 {
                                     var estimated = new TimeSpan((long)Math.Floor((double)elapsed.Ticks / transferred * TotalBytes));
                                     RemainingTime = estimated - elapsed;
                                 }
                                 if (args.Percentage == -1)
                                 {
                                     if (_speedMeter.IsRunning)
                                         _speedMeter.Restart();
                                     else
                                         _speedMeter.Start();  
                                 } 
                                 else
                                 {
                                     if (args.Percentage == 100)
                                     {
                                         _speedMeter.Stop();
                                     } 
                                     else if (!_speedMeter.IsRunning)
                                     {
                                         _speedMeter.Restart();
                                     }
                                     var ms = _speedMeter.Elapsed.TotalMilliseconds;
                                     Speed = (int)Math.Floor(args.TotalBytesTransferred/ms*1000/1024);
                                     if (Speed < 0) Debug.WriteLine("{0} {1} {2}", args.Transferred, ms, Speed);
                                 }
                                 CurrentFileProgress = args.Percentage > 0 ? args.Percentage : 0;
                                 BytesTransferred += args.Transferred;
                             });
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
                LeftPane.Dispose();
                LeftPane = _leftPaneStack.Pop();
                LeftPane.LoadDataAsync(LoadCommand.Restore, args.Payload);
                
            }
            else if (RightPane == args.Pane)
            {
                RightPane.Dispose();
                RightPane = _rightPaneStack.Pop();
                RightPane.LoadDataAsync(LoadCommand.Restore, args.Payload);
            }
        }

        private void InitializeTransfer(TransferType mode, Action callback)
        {
            TelnetException ex = null;
            WorkerThread.Run(
                () =>
                    {
                        var targetPane = TargetPane as LocalFileSystemContentViewModel;
                        if (targetPane != null)
                        {
                            if (targetPane.IsNetworkDrive && UserSettings.Get<bool>(UserSettings.UseRemoteCopy))
                            {
                                try
                                {
                                    OpenTelnetSession(targetPane, (FtpContentViewModel) SourcePane);
                                    return CopyMode.RemoteExport;
                                }
                                catch (TelnetException exception)
                                {
                                    ex = exception;
                                }
                            }
                            return CopyMode.DirectExport;
                        }

                        var sourcePane = SourcePane as LocalFileSystemContentViewModel;
                        if (sourcePane != null)
                        {
                            if (sourcePane.IsNetworkDrive && UserSettings.Get<bool>(UserSettings.UseRemoteCopy))
                            {
                                try
                                {
                                    OpenTelnetSession(sourcePane, (FtpContentViewModel) TargetPane);
                                    return CopyMode.RemoteImport;
                                }
                                catch (TelnetException exception)
                                {
                                    ex = exception;
                                }                                
                            }
                            return CopyMode.DirectImport;
                        }
                        return CopyMode.Indirect;
                    },
                copyMode =>
                    {
                        if (ex != null)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine(string.Format("Cannot connect to remote server [{0}]", ex.Host));
                            sb.AppendLine();
                            Exception e = ex;
                            while (e != null)
                            {
                                sb.AppendLine(e.Message);
                                e = ex.InnerException;
                            }
                            sb.AppendLine();
                            sb.AppendLine("Please check the server supports Direct Copy or turn Direct Copy off.");
                            sb.AppendLine("Now the selected file(s) will be transfered indirectly.");
                            NotificationMessage.Show("Session initiation failed", sb.ToString());
                        }
                        _copyMode = copyMode;
                        TransferType = mode;
                        _rememberedCopyAction = CopyAction.CreateNew;
                        FileCount = _queue.Count;
                        if (mode == TransferType.Move) FileCount /= 2;
                        TotalBytes =
                            _queue.Where(item => item.FileSystemItem.Type == ItemType.File)
                                  .Sum(item => item.FileSystemItem.Size ?? 0);
                        FilesTransferred = 0;
                        BytesTransferred = 0;
                        ProgressState = TaskbarItemProgressState.Normal;
                        NotifyTransferStarted();
                        _elapsedTimeMeter.Start();
                        callback.Invoke();
                    });
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
            _isAborted = true;
            SourcePane.Abort();
            TargetPane.Abort();
            lock (_queue)
            {
                var actualItem = _queue.Peek();
                _queue.Clear();
                _queue.Enqueue(actualItem);
            }
        }

        private void FinishTransfer()
        {
            if (_copyMode == CopyMode.RemoteExport || _copyMode == CopyMode.RemoteImport) CloseTelnetSession();
            _speedMeter.Stop();
            _speedMeter.Reset();
            _elapsedTimeMeter.Stop();
            _elapsedTimeMeter.Reset();
            ProgressState = TaskbarItemProgressState.None;
            NotifyTransferFinished();
        }

        private void RenameExistingFile(TransferException exception, CopyAction? action, Action<CopyAction?, string> rename, Action<Exception> chooseDifferentOption)
        {
            var name = InputDialog.Show("Rename", "New name:", Path.GetFileName(exception.TargetFile));
            if (!string.IsNullOrEmpty(name))
            {
                rename.Invoke(action, name);
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

        public override void Dispose()
        {
            LeftPane.Dispose();
            var left = _leftPaneStack.LastOrDefault() ?? LeftPane;
            UserSettings.Save(UserSettings.LeftPaneType, left.GetType().FullName);
            UserSettings.Save(UserSettings.LeftPaneFileListPaneSettings, LeftPane.Settings);
            RightPane.Dispose();
            var right = _rightPaneStack.LastOrDefault() ?? RightPane;
            UserSettings.Save(UserSettings.RightPaneType, right.GetType().FullName);
            UserSettings.Save(UserSettings.RightPaneFileListPaneSettings, RightPane.Settings);
            base.Dispose();
        }
    }
}