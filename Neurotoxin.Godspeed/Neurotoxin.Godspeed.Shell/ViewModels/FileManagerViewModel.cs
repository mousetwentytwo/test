﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
using Microsoft.Practices.ObjectBuilder2;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class FileManagerViewModel : ViewModelBase
    {
        private const string RenameFromPattern = @"([\/]){0}$";
        private const string RenameToPattern = @"${{1}}{0}";
        private readonly Stopwatch _speedMeter = new Stopwatch();
        private readonly Stopwatch _elapsedTimeMeter = new Stopwatch();
        private Queue<QueueItem> _queue;
        private CopyMode _copyMode;
        private CopyAction _rememberedCopyAction;
        private TransferErrorDialogResult _skipAll;
        private long _currentFileBytesTransferred;
        private bool _isAborted;
        private bool _isContinued;
        private readonly StatisticsViewModel _statistics;

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

        private const string ACTIVEPANE = "ActivePane";
        public IPaneViewModel ActivePane
        {
            get 
            { 
                if (LeftPane != null && LeftPane.IsActive) return LeftPane;
                if (RightPane != null && RightPane.IsActive) return RightPane;
                return null;
            }
        }

        private const string OTHERPANE = "OtherPane";
        public IPaneViewModel OtherPane
        {
            get { return LeftPane.IsActive ? RightPane : LeftPane; }
        }

        private const string SOURCEPANE = "SourcePane";
        public IFileListPaneViewModel SourcePane
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

        private const string TARGETPANE = "TargetPane";
        public IFileListPaneViewModel TargetPane
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

        private FtpContentViewModel Ftp
        {
            get
            {
                return SourcePane as FtpContentViewModel ?? TargetPane as FtpContentViewModel;
            }
        }

        private const string UNREADMESSAGECOUNT = "UnreadMessageCount";
        public int UnreadMessageCount
        {
            get { return UserMessages.Count(m => !m.IsRead); }
        }

        public ObservableCollection<IUserMessageViewModel> UserMessages { get; private set; }

        #endregion

        #region Transfer properties

        private const string TRANSFERTYPE = "TransferType";
        private TransferType _transferType;
        public TransferType TransferType
        {
            get { return _transferType; }
            set { _transferType = value; NotifyPropertyChanged(TRANSFERTYPE); }
        }

        private const string TRANSFERACTION = "TransferAction";
        private string _transferAction;
        public string TransferAction
        {
            get { return _transferAction; }
            set { _transferAction = value; NotifyPropertyChanged(TRANSFERACTION); }
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
                        return TotalBytes != 0 ? (int)(BytesTransferred * 100 / TotalBytes) : 0;
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

        private const string ISPAUSED = "IsPaused";
        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; NotifyPropertyChanged(ISPAUSED); }
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
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy &&
                   !TargetPane.IsBusy && !TargetPane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteCopyCommand()
        {
            if (!ConfirmCommand(TransferType.Copy, SourcePane.SelectedItems.Count() > 1)) return;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Copy), CopyPrepare, PopulationError);
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
                SourceFile = !string.IsNullOrEmpty(SourcePane.CurrentFolder.Path)
                                 ? item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty)
                                 : item.Path;
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
                case ItemType.Link:
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
                            var tempFile = Path.Combine(App.DataDirectory, "temp", item.FullPath.Hash());
                            var tempItem = item.Clone();
                            tempItem.Path = tempFile;
                            var result = SourcePane.Export(item, tempFile, CopyAction.Overwrite) &&
                                         TargetPane.Import(tempItem, targetPath, action ?? _rememberedCopyAction);
                            File.Delete(tempFile);
                            return result;
                        case CopyMode.RemoteExport:
                            if (new Regex(@"[^\x20-\x7f]").IsMatch(targetPath))
                            {
                                //TODO: not sure this is the right idea
                                _copyMode = CopyMode.DirectExport;
                                CloseTelnetSession();
                                eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                                return SourcePane.Export(item, targetPath, a);
                            }
                            return ((FtpContentViewModel)SourcePane).RemoteDownload(item, targetPath, a);
                        case CopyMode.RemoteImport:
                            if (new Regex(@"[^\x20-\x7f]").IsMatch(item.Path))
                            {
                                //TODO: not sure this is the right idea
                                _copyMode = CopyMode.DirectImport;
                                CloseTelnetSession();
                                eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                                return TargetPane.Import(item, targetPath, a);
                            }
                            return ((FtpContentViewModel)TargetPane).RemoteUpload(item, targetPath, a);
                        default:
                            throw new NotSupportedException("Invalid Copy Mode: " + _copyMode);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private void CopySuccess(bool result)
        {
            if (IsPaused)
            {
                Pause();
                return;
            }

            var item = _queue.Dequeue().FileSystemItem;
            FilesTransferred++;
            if (item.Type == ItemType.File && result) _statistics.FilesTransferred++;
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path.StartsWith(i.Path));
            if (vm != null)
            {
                if (vm.Type == ItemType.File || !_queue.Any(q => q.FileSystemItem.Path.StartsWith(vm.Path)))
                    vm.IsSelected = false;
            }
            _currentFileBytesTransferred = 0;
            CopyStart();
        }

        private void CopyError(Exception exception)
        {
            if (_isAborted) { CopyFinish(); return; }
            if (IsPaused) return;

            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    BytesTransferred -= _currentFileBytesTransferred;
                    _currentFileBytesTransferred = 0;
                    CopyStart(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, CopyStart, CopyError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    BytesTransferred += (_queue.Peek().FileSystemItem.Size ?? 0) - _currentFileBytesTransferred;
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
            _statistics.TimeSpentWithTransfer += _elapsedTimeMeter.Elapsed;
            TargetPane.Refresh();
        }

        private void OpenTelnetSession(LocalFileSystemContentViewModel local, FtpContentViewModel ftp)
        {
            var connection = ftp.Connection;
            Telnet.OpenSession(local.Drive.Path, local.Drive.FullPath, connection.Address, connection.Port ?? 21, connection.Username, connection.Password);
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
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy &&
                   !TargetPane.IsBusy && !TargetPane.IsReadOnly && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteMoveCommand()
        {
            if (!ConfirmCommand(TransferType.Move, SourcePane.SelectedItems.Count() > 1)) return;
            AsyncJob(() => SourcePane.PopulateQueue(TransferType.Move), MovePrepare, PopulationError);
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
            if (IsPaused)
            {
                Pause();
                return;
            }

            var queueitem = _queue.Dequeue();
            var item = queueitem.FileSystemItem;
            if (queueitem.TransferType == TransferType.Copy)
            {
                FilesTransferred++;
                if (result) _statistics.FilesTransferred++;
            }
            var vm = SourcePane.SelectedItems.FirstOrDefault(i => item.Path == i.Path);
            if (vm != null) vm.IsSelected = false;
            _currentFileBytesTransferred = 0;
            MoveStart();
        }

        private void MoveError(Exception exception)
        {
            if (_isAborted) { MoveFinish(); return; }
            if (IsPaused) return;

            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    BytesTransferred -= _currentFileBytesTransferred;
                    _currentFileBytesTransferred = 0;
                    MoveStart(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, MoveStart, MoveError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    BytesTransferred += _queue.Peek().FileSystemItem.Size ?? 0 - _currentFileBytesTransferred;
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
            _statistics.TimeSpentWithTransfer += _elapsedTimeMeter.Elapsed;
            SourcePane.Refresh();
            TargetPane.Refresh();
        }

        #endregion

        #region NewFolderCommand

        public DelegateCommand NewFolderCommand { get; private set; }

        private bool CanExecuteNewFolderCommand()
        {
            return SourcePane != null && !SourcePane.IsBusy && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
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

            var name = InputDialog.Show(Resx.AddNewFolder, Resx.FolderName + Strings.Colon, string.Empty, suggestion);
            if (string.IsNullOrEmpty(name))
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithNoName);
                return;
            }
            var invalidChars = Path.GetInvalidFileNameChars();
            if (invalidChars.Any(name.Contains))
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithInvalidCharacters);
                return;
            }
            var path = string.Format("{0}{1}", SourcePane.CurrentFolder.Path, name);
            WorkerThread.Run(() => SourcePane.CreateFolder(path), success => NewFolderSuccess(success, name), NewFolderError);
        }

        private void NewFolderSuccess(bool success, string name)
        {
            if (!success)
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, string.Format(Resx.FolderAlreadyExists, name));
                return;
            }
            SourcePane.Refresh(() =>
                                   {
                                       SourcePane.CurrentRow = SourcePane.Items.Single(item => item.Name == name);
                                   });
        }

        private void NewFolderError(Exception ex)
        {
            ShowCorrespondingErrorDialog(ex, false);
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand DeleteCommand { get; private set; }

        private bool CanExecuteDeleteCommand()
        {
            var connections = ActivePane as ConnectionsViewModel;
            if (connections != null)
            {
                return connections.SelectedItem != null && !connections.IsBusy;
            }
            return SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteDeleteCommand()
        {
            var connections = ActivePane as ConnectionsViewModel;
            if (!ConfirmCommand(TransferType.Delete, connections == null && SourcePane.SelectedItems.Count() > 1)) return;
            if (connections != null)
            {
                connections.Delete();
            }
            else
            {
                _isAborted = false;
                AsyncJob(() => SourcePane.PopulateQueue(TransferType.Delete), DeletePrepare, PopulationError);
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
            if (_isAborted) { DeleteFinish(); return; }
            if (IsPaused) return;

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

        #region PauseCommand

        public DelegateCommand PauseCommand { get; private set; }

        private void ExecutePauseCommand()
        {
            IsPaused = true;
            SourcePane.Abort();
            TargetPane.Abort();
        }

        private void Pause()
        {
            _speedMeter.Stop();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.Indeterminate;
        }

        #endregion

        #region ContinueCommand

        public DelegateCommand ContinueCommand { get; private set; }

        private void ExecuteContinueCommand()
        {
            _elapsedTimeMeter.Start();
            ProgressState = TaskbarItemProgressState.Normal;
            IsPaused = false;
            _isContinued = true;
            switch (TransferType)
            {
                case TransferType.Copy:
                    CopyStart(CopyAction.Resume);
                    break;
                case TransferType.Move:
                    MoveStart(CopyAction.Resume);
                    break;
                default:
                    DeleteStart();
                    break;
            }
        }

        #endregion

        #region OpenUserMessageCommand

        public DelegateCommand<UserMessageCommandParameter> OpenUserMessageCommand { get; private set; }

        private void ExecuteOpenUserMessageCommand(UserMessageCommandParameter p)
        {
            var message = p.ViewModel;
            message.IsRead = true;
            message.IsChecked = true;
            NotifyPropertyChanged(UNREADMESSAGECOUNT);

            switch (p.Command)
            {
                case MessageCommand.OpenUrl:
                    Process.Start((string)p.Parameter);
                    if (message.Flags.HasFlag(MessageFlags.IgnoreAfterOpen)) UserSettings.IgnoreMessage(message.Message);
                    break;
                case MessageCommand.OpenDialog:
                    var dialogType = (Type) p.Parameter;
                    var c = dialogType.GetConstructor(Type.EmptyTypes);
                    var d = c.Invoke(new object[0]) as Window;
                    if (d.ShowDialog() == true) UserSettings.IgnoreMessage(message.Message);
                    break;
            }
        }

        #endregion

        #region RemoveUserMessageCommand

        public DelegateCommand<UserMessageViewModel> RemoveUserMessageCommand { get; private set; }

        private void ExecuteRemoveUserMessageCommand(UserMessageViewModel message)
        {
            if (message.Flags.HasFlag(MessageFlags.Ignorable) && 
                new ConfirmationDialog(Resx.RemoveUserMessage, Resx.RemoveUserMessageConfirmation).ShowDialog() == true)
            {
                UserSettings.IgnoreMessage(message.Message);
            }
            UserMessages.Remove(message);
            if (UserMessages.Count == 0) UserMessages.Add(new NoMessagesViewModel());
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

        public FileManagerViewModel(StatisticsViewModel statistics)
        {
            _statistics = statistics;
            UserMessages = new ObservableCollection<IUserMessageViewModel> { new NoMessagesViewModel() };
            UserMessages.CollectionChanged += (sender, args) => NotifyPropertyChanged(UNREADMESSAGECOUNT);

            SwitchPaneCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSwitchPaneCommand, CanExecuteSwitchPaneCommand);
            EditCommand = new DelegateCommand(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            PauseCommand = new DelegateCommand(ExecutePauseCommand);
            ContinueCommand = new DelegateCommand(ExecuteContinueCommand);
            OpenUserMessageCommand = new DelegateCommand<UserMessageCommandParameter>(ExecuteOpenUserMessageCommand);
            RemoveUserMessageCommand = new DelegateCommand<UserMessageViewModel>(ExecuteRemoveUserMessageCommand);

            eventAggregator.GetEvent<TransferActionStartedEvent>().Subscribe(OnTransferActionStarted);
            eventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
            eventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnOpenNestedPane);
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Subscribe(OnCloseNestedPane);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(OnActivePaneChanged);
            eventAggregator.GetEvent<NotifyUserMessageEvent>().Subscribe(OnNotifyUserMessage);
        }

        public void Initialize()
        {
            LeftPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(UserSettings.LeftPaneType));
            var leftParam = UserSettings.LeftPaneFileListPaneSettings;
            LeftPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(leftParam), PaneLoaded);

            RightPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(UserSettings.RightPaneType));
            var rightParam = UserSettings.RightPaneFileListPaneSettings;
            RightPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(rightParam), PaneLoaded);
        }

        private void PaneLoaded(PaneViewModelBase pane)
        {
            if (!LeftPane.IsLoaded || !RightPane.IsLoaded) return;
            eventAggregator.GetEvent<ShellInitializedEvent>().Publish(new ShellInitializedEventArgs());
        }

        private static Type GetStoredPaneType(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
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

        public void SetUserMessagesToRead(IUserMessageViewModel[] items)
        {
            items.ForEach(item => item.IsRead = true);
            NotifyPropertyChanged(UNREADMESSAGECOUNT);
        }

        private void OnTransferActionStarted(string action)
        {
            UIThread.Run(() => { TransferAction = action ?? Resx.ResourceManager.GetString(TransferType.ToString()); });
        }

        private void OnTransferProgressChanged(TransferProgressChangedEventArgs args)
        {
            if (_isContinued)
            {
                _isContinued = false;
                return;
            }
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

                    if (args.Percentage == 100)
                    {
                        _speedMeter.Stop();
                        _statistics.BytesTransferred += args.TotalBytesTransferred - args.ResumeStartPosition;
                    } 
                    else if (!_speedMeter.IsRunning)
                    {
                        _speedMeter.Restart();
                    }
                    var ms = _speedMeter.Elapsed.TotalMilliseconds;
                    if (ms > 100) Speed = (int)Math.Floor((args.TotalBytesTransferred - args.ResumeStartPosition)/ms*1000/1024);
                    CurrentFileProgress = args.Percentage > 0 ? args.Percentage : 0;
                    _currentFileBytesTransferred += args.Transferred;
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
                var settings = LeftPane.Settings;
                LeftPane.Dispose();
                LeftPane = _leftPaneStack.Pop();
                LeftPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                LeftPane.SetActive();
            }
            else if (RightPane == args.Pane)
            {
                var settings = LeftPane.Settings;
                RightPane.Dispose();
                RightPane = _rightPaneStack.Pop();
                RightPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                RightPane.SetActive();
            }
        }

        private void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            NotifyPropertyChanged(ACTIVEPANE);
        }

        private void OnNotifyUserMessage(NotifyUserMessageEventArgs e)
        {
            if (!UIThread.IsUIThread)
            {
                UIThread.Run(() => OnNotifyUserMessage(e));
                return;
            }
            if (UserSettings.IsMessageIgnored(e.MessageKey)) return;
            var message = string.Format(Resx.ResourceManager.GetString(e.MessageKey), e.MessageArgs);
            var i = UserMessages.IndexOf(m => m.Message == message);
            if (i == -1)
            {
                if (UserMessages.First() is NoMessagesViewModel) UserMessages.RemoveAt(0);
                UserMessages.Insert(0, new UserMessageViewModel(message, e));
            } 
            else if (i != 0)
            {
                UserMessages.Move(i, 0);
            }
        }

        private void InitializeTransfer(TransferType mode, Action callback)
        {
            TransferType = mode;
            TransferAction = Resx.ResourceManager.GetString(mode.ToString());
            _rememberedCopyAction = CopyAction.CreateNew;
            _currentFileBytesTransferred = 0;
            CurrentFileProgress = 0;
            FilesTransferred = 0;
            FileCount = _queue.Count;
            if (mode == TransferType.Move) FileCount /= 2;
            BytesTransferred = 0;
            TotalBytes = _queue.Where(item => item.FileSystemItem.Type == ItemType.File).Sum(item => item.FileSystemItem.Size ?? 0);
            Speed = 0;
            ElapsedTime = new TimeSpan(0);
            RemainingTime = new TimeSpan(0);

            TelnetException ex = null;
            WorkerThread.Run(
                () =>
                    {
                        if (mode == TransferType.Delete) return CopyMode.Invalid;

                        var targetPane = TargetPane as LocalFileSystemContentViewModel;
                        if (targetPane != null)
                        {
                            if (targetPane.IsNetworkDrive && UserSettings.UseRemoteCopy)
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
                            if (sourcePane.IsNetworkDrive && UserSettings.UseRemoteCopy)
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
                            var dialog = new RemoteCopyErrorDialog(ex);
                            if (dialog.ShowDialog() == false) return;
                            if (dialog.TurnOffRemoteCopy) UserSettings.UseRemoteCopy = false;
                        }
                        if (copyMode == CopyMode.Indirect) TotalBytes *= 2;
                        _copyMode = copyMode;
                        ProgressState = TaskbarItemProgressState.Normal;
                        NotifyTransferStarted();
                        _elapsedTimeMeter.Reset();
                        _elapsedTimeMeter.Start();
                        callback.Invoke();
                    });
        }

        internal TransferErrorDialogResult ShowCorrespondingErrorDialog(Exception exception, bool feedbackNeeded = true)
        {
            _elapsedTimeMeter.Stop();
            var transferException = exception as TransferException;
            var result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
            if (transferException != null)
            {
                switch (transferException.Type)
                {
                    case TransferErrorType.NotSpecified:
                        {
                            if (feedbackNeeded)
                            {
                                var dialog = new IoErrorDialog(exception);
                                if (dialog.ShowDialog() == true)
                                {
                                    result = dialog.Result;
                                    if (TargetPane != null) result.Action = TargetPane.IsResumeSupported ? CopyAction.Resume : CopyAction.Overwrite;
                                }
                            } 
                            else
                            {
                                NotificationMessage.ShowMessage(Resx.IOError, exception.Message);
                            }
                        }
                        break;
                    case TransferErrorType.WriteAccessError:
                        {
                            if (_skipAll != null)
                            {
                                result = _skipAll;
                            }
                            else
                            {
                                var sourceFile = _queue.Peek().FileSystemItem;
                                var dialog = new WriteErrorDialog(eventAggregator, sourceFile.Path, transferException.TargetFile, TargetPane.IsResumeSupported && sourceFile.Size > transferException.TargetFileSize);
                                SourcePane.GetItemViewModel(sourceFile.Path);
                                TargetPane.GetItemViewModel(transferException.TargetFile);
                                if (dialog.ShowDialog() == true) result = dialog.Result;
                            }
                        }
                        break;
                    case TransferErrorType.LostConnection:
                        result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
                        var ftp = LeftPane as FtpContentViewModel ?? RightPane as FtpContentViewModel;
                        var reconnectionDialog = new ReconnectionDialog(exception);
                        if (reconnectionDialog.ShowDialog() == true)
                        {
                            try
                            {
                                ftp.RestoreConnection();
                                ftp.Refresh();
                            } 
                            catch (Exception ex)
                            {
                                NotificationMessage.ShowMessage(Resx.ConnectionFailed, string.Format(Resx.CannotReestablishConnection, ex.Message));
                                ftp.CloseCommand.Execute();
                            }
                        }
                        else
                        {
                            ftp.CloseCommand.Execute();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Invalid transfer error type: " + transferException.Type);
                }

                //TODO: refactor scoping
                if (result.Scope == CopyActionScope.All)
                {
                    if (result.Action.HasValue) _rememberedCopyAction = result.Action.Value;
                    if (result.Behavior == ErrorResolutionBehavior.Skip) _skipAll = result;
                }
            }
            else
            {
                ErrorMessage.Show(exception);
            }
            _elapsedTimeMeter.Start();
            return result;
        }

        internal void AbortTransfer()
        {
            _isAborted = true;
            if (_copyMode != CopyMode.RemoteExport && _copyMode != CopyMode.RemoteImport)
            {
                SourcePane.Abort();
                TargetPane.Abort();
            }
            lock (_queue)
            {
                var actualItem = _queue.Peek();
                _queue.Clear();
                _queue.Enqueue(actualItem);
            }
            if (_isPaused)
            {
                UIThread.BeginRun(() =>
                {
                    switch (TransferType)
                    {
                        case TransferType.Copy:
                            CopyFinish();
                            break;
                        case TransferType.Move:
                            MoveFinish();
                            break;
                        case TransferType.Delete:
                            DeleteFinish();
                            break;
                    }
                });
            }
        }

        private void FinishTransfer()
        {
            if (_copyMode == CopyMode.RemoteExport || _copyMode == CopyMode.RemoteImport) CloseTelnetSession();
            SourcePane.FinishTransferAsSource();
            TargetPane.FinishTransferAsTarget();
            if (Ftp != null) Ftp.IsKeepAliveEnabled = false;
            _speedMeter.Stop();
            _speedMeter.Reset();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.None;
            NotifyTransferFinished();
        }

        private static void RenameExistingFile(TransferException exception, CopyAction? action, Action<CopyAction?, string> rename, Action<Exception> chooseDifferentOption)
        {
            var name = InputDialog.Show(Resx.Rename, Resx.NewName + Strings.Colon , Path.GetFileName(exception.TargetFile));
            if (!string.IsNullOrEmpty(name))
            {
                rename.Invoke(action, name);
            }
            else
            {
                chooseDifferentOption.Invoke(exception);
            }
        }

        private bool ConfirmCommand(TransferType type, bool isPlural)
        {
            var title = Resx.ResourceManager.GetString(type.ToString());
            var message = Resx.ResourceManager.GetString(string.Format(isPlural ? "{0}ConfirmationPlural" : "{0}ConfirmationSingular", type));
            if (new ConfirmationDialog(title, message).ShowDialog() != true) return false;
            _isAborted = false;
            if (Ftp != null) Ftp.IsKeepAliveEnabled = true;
            return true;
        }

        private static void AsyncJob<T>(Func<T> work, Action<T> success, Action<Exception> error = null)
        {
            var finished = false;
            WorkerThread.Run(
                () =>
                    {
                        Thread.Sleep(3000); 
                        return true;
                    }, 
                b =>
                    {
                        if (finished) return;
                        NotificationMessage.ShowMessage(Resx.ApplicationIsBusy, Resx.PleaseWait, NotificationMessageFlags.NonClosable);
                    });
            WorkerThread.Run(work, 
                b =>
                    {
                        NotificationMessage.CloseMessage();
                        finished = true;
                        success.Invoke(b);
                    }, 
                e =>
                    {
                        NotificationMessage.CloseMessage();
                        finished = true;
                        if (error != null) error.Invoke(e);
                    });
        }

        private static void PopulationError(Exception ex)
        {
            ErrorMessage.Show(new SomethingWentWrongException(Resx.PopulationFailed, ex));
        }

        public override void Dispose()
        {
            object data = null;
            IPaneViewModel left;
            do
            {
                data = LeftPane.Close(data);
                left = LeftPane;
                LeftPane = _leftPaneStack.Count > 0 ? _leftPaneStack.Pop() : null;
            } 
            while (LeftPane != null);  
            UserSettings.LeftPaneType = left.GetType().FullName;
            UserSettings.LeftPaneFileListPaneSettings = left.Settings;

            data = null;
            IPaneViewModel right;
            do
            {
                data = RightPane.Close(data);
                right = RightPane;
                RightPane = _rightPaneStack.Count > 0 ? _rightPaneStack.Pop() : null;
            }
            while (RightPane != null);
            UserSettings.RightPaneType = right.GetType().FullName;
            UserSettings.RightPaneFileListPaneSettings = right.Settings;

            base.Dispose();
        }
    }
}