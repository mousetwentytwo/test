using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Shell;
using Neurotoxin.Godspeed.Core.Exceptions;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using System.Linq;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class TransferManagerViewModel : CommonViewModelBase
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
        private bool _sourceChanged;
        private bool _targetChanged;

        private readonly IStatisticsViewModel _statistics;
        private readonly IUserSettings _userSettings;

        #region Properties

        public IFileListPaneViewModel SourcePane { get; private set; }
        public IFileListPaneViewModel TargetPane { get; private set; }

        private const string USERACTION = "UserAction";
        private FileOperation _userAction;
        public FileOperation UserAction
        {
            get { return _userAction; }
            set { _userAction = value; NotifyPropertyChanged(USERACTION); }
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
                switch (UserAction)
                {
                    case FileOperation.Delete:
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

        public TransferManagerViewModel(IUserSettings userSettings, IStatisticsViewModel statistics)
        {
            _userSettings = userSettings;
            _statistics = statistics;
            eventAggregator.GetEvent<TransferActionStartedEvent>().Subscribe(OnTransferActionStarted);
            eventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
            eventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Subscribe(OnShowCorrespondingError);
        }

        public void Copy(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane)
        {
            SourcePane = sourcePane;
            TargetPane = targetPane;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Copy), queue => InitializeTransfer(queue, FileOperation.Copy), PopulationError);
        }

        public void Move(IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane)
        {
            SourcePane = sourcePane;
            TargetPane = targetPane;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Copy), queue => InitializeTransfer(queue, FileOperation.Move), PopulationError);
        }

        public void Delete(IFileListPaneViewModel sourcePane)
        {
            SourcePane = sourcePane;
            AsyncJob(() => SourcePane.PopulateQueue(FileOperation.Delete), queue => InitializeTransfer(queue, FileOperation.Delete), PopulationError);
        }

        private OperationResult ExecuteCopy(QueueItem queueitem, CopyAction? action, string rename)
        {
            var item = queueitem.FileSystemItem;
            var sourcePath = item.GetRelativePath(SourcePane.CurrentFolder.Path);
            var targetPath = TargetPane.GetTargetPath(sourcePath);
            TransferResult result;

            switch (item.Type)
            {
                case ItemType.Directory:
                case ItemType.Link:
                    TargetPane.CreateFolder(targetPath);
                    result = TransferResult.Ok;
                    break;
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
                            result = SourcePane.Export(item, targetPath, a);
                            break;
                        case CopyMode.DirectImport:
                            result = TargetPane.Import(item, targetPath, a);
                            break;
                        case CopyMode.Indirect:
                            var tempFile = Path.Combine(App.DataDirectory, "temp", item.FullPath.Hash());
                            var tempItem = item.Clone();
                            tempItem.Path = tempFile;
                            var export = SourcePane.Export(item, tempFile, CopyAction.Overwrite);
                            var import = TransferResult.Skipped;
                            if (export == TransferResult.Ok)
                                import = TargetPane.Import(tempItem, targetPath, action ?? _rememberedCopyAction);
                            File.Delete(tempFile);
                            result = export != TransferResult.Ok ? export : import;
                            break;
                        case CopyMode.RemoteExport:
                            if (new Regex(@"[^\x20-\x7f]").IsMatch(targetPath))
                            {
                                //TODO: not sure this is the right idea
                                _copyMode = CopyMode.DirectExport;
                                CloseTelnetSession();
                                eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                                result = SourcePane.Export(item, targetPath, a);
                            } 
                            else
                            {
                                result = ((FtpContentViewModel)SourcePane).RemoteDownload(item, targetPath, a);
                            }
                            break;
                        case CopyMode.RemoteImport:
                            if (new Regex(@"[^\x20-\x7f]").IsMatch(item.Path))
                            {
                                //TODO: not sure this is the right idea
                                _copyMode = CopyMode.DirectImport;
                                CloseTelnetSession();
                                eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                                result = TargetPane.Import(item, targetPath, a);
                            } else
                            {
                                result = ((FtpContentViewModel)TargetPane).RemoteUpload(item, targetPath, a);
                            }
                            break;
                        default:
                            throw new NotSupportedException("Invalid Copy Mode: " + _copyMode);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new OperationResult(result, targetPath);
        }

        private OperationResult ExecuteDelete(QueueItem item)
        {
            return new OperationResult(SourcePane.Delete(item.FileSystemItem));
        }

        private OperationResult ExecuteVerification(QueueItem item)
        {
            var ftp = TargetPane as FtpContentViewModel;
            if (ftp == null) throw new NotSupportedException("Target pane does not support file hash verification"); //TODO: ResX
            UIThread.Run(() => OnTransferActionStarted(Resx.Verifying));
            var verificationResult = ftp.VerifyUpload((string)item.Payload, item.FileSystemItem.Path);
            return new OperationResult(verificationResult);
        }

        private void ProcessQueueItem(CopyAction? action = null, string rename = null)
        {
            if (_queue.Count > 0)
            {
                var queueitem = _queue.Peek();
                var item = queueitem.FileSystemItem;
                SourceFile = item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkerThread.Run(() =>
                {
                    switch (queueitem.Operation)
                    {
                        case FileOperation.Copy:
                            return ExecuteCopy(queueitem, action, rename);
                        case FileOperation.Delete:
                            return ExecuteDelete(queueitem);
                        case FileOperation.Verify:
                            return ExecuteVerification(queueitem);
                        default:
                            throw new NotSupportedException("Invalid transfer type: " + queueitem.Operation);
                    }
                }, ProcessSuccess, ProcessError);
                return;
            }
            FinishTransfer();
        }

        private void ProcessSuccess(OperationResult result)
        {
            FileSystemItem deselect = null;
            var queueItem = _queue.Peek();
            switch (result.Result)
            {
                case TransferResult.Ok:
                    if (queueItem.Operation == FileOperation.Copy && TargetPane.IsVerificationEnabled)
                    {
                        _targetChanged = true;
                        queueItem.Operation = FileOperation.Verify;
                        queueItem.Payload = result.TargetPath;
                    }
                    else if (queueItem.Operation != FileOperation.Delete && UserAction == FileOperation.Move)
                    {
                        if (queueItem.FileSystemItem.Type == ItemType.Directory)
                        {
                            _queue.Dequeue();
                            _queue.Enqueue(new QueueItem(queueItem.FileSystemItem, FileOperation.Delete));
                        }
                        else
                        {
                            queueItem.Operation = FileOperation.Delete;
                        }
                    }
                    else
                    {
                        if (queueItem.Operation == FileOperation.Delete) _sourceChanged = true;
                        if (UserAction != FileOperation.Delete) _statistics.FilesTransferred++;
                        FilesTransferred++;
                        deselect = queueItem.FileSystemItem;
                        _queue.Dequeue();
                    }
                    break;
                case TransferResult.Skipped:
                case TransferResult.Aborted:
                    _queue.Dequeue();
                    break;
            }

            _currentFileBytesTransferred = 0;

            if (deselect != null)
            {
                var vm = SourcePane.SelectedItems.FirstOrDefault(i => deselect.Path.StartsWith(i.Path));
                if (vm != null && (vm.Type == ItemType.File || !_queue.Any(q => q.FileSystemItem.Path.StartsWith(vm.Path))))
                {
                    vm.IsSelected = false;
                }
            }

            if (IsPaused)
            {
                PauseMeters();
            } 
            else
            {
                ProcessQueueItem();
            }
        }

        private void ProcessError(Exception exception)
        {
            if (_isAborted) { FinishTransfer(); return; }
            if (IsPaused) return;

            var result = ShowCorrespondingErrorDialog(exception);
            switch (result.Behavior)
            {
                case ErrorResolutionBehavior.Retry:
                    BytesTransferred -= _currentFileBytesTransferred;
                    _currentFileBytesTransferred = 0;
                    ProcessQueueItem(result.Action);
                    break;
                case ErrorResolutionBehavior.Rename:
                    RenameExistingFile((TransferException)exception, CopyAction.Rename, ProcessQueueItem, ProcessError);
                    break;
                case ErrorResolutionBehavior.Skip:
                    BytesTransferred += _queue.Peek().FileSystemItem.Size ?? 0 - _currentFileBytesTransferred;
                    ProcessSuccess(new OperationResult(TransferResult.Skipped));
                    break;
                case ErrorResolutionBehavior.Cancel:
                    FinishTransfer();
                    break;
            }
        }

        private void OnTransferActionStarted(string action)
        {
            UIThread.Run(() => { TransferAction = action ?? Resx.ResourceManager.GetString(UserAction.ToString()); });
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

        private void OnShowCorrespondingError(ShowCorrespondingErrorEventArgs e)
        {
            ShowCorrespondingErrorDialog(e.Exception, e.FeedbackNeeded);
        }

        public void InitializeTransfer(Queue<QueueItem> queue, FileOperation mode)
        {
            _queue = queue;
            _isAborted = false;
            UserAction = mode;
            TransferAction = Resx.ResourceManager.GetString(mode.ToString());
            _rememberedCopyAction = CopyAction.CreateNew;
            _currentFileBytesTransferred = 0;
            CurrentFileProgress = 0;
            FilesTransferred = 0;
            FileCount = _queue.Count;
            BytesTransferred = 0;
            TotalBytes = _queue.Where(item => item.FileSystemItem.Type == ItemType.File).Sum(item => item.FileSystemItem.Size ?? 0);
            Speed = 0;
            ElapsedTime = new TimeSpan(0);
            RemainingTime = new TimeSpan(0);

            TelnetException ex = null;
            WorkerThread.Run(
                () =>
                    {
                        if (mode == FileOperation.Delete) return CopyMode.Invalid;

                        var targetPane = TargetPane as LocalFileSystemContentViewModel;
                        if (targetPane != null)
                        {
                            if (targetPane.IsNetworkDrive && _userSettings.UseRemoteCopy)
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
                            if (sourcePane.IsNetworkDrive && _userSettings.UseRemoteCopy)
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
                            if (dialog.TurnOffRemoteCopy) _userSettings.UseRemoteCopy = false;
                        }
                        if (copyMode == CopyMode.Indirect) TotalBytes *= 2;
                        _copyMode = copyMode;
                        ProgressState = TaskbarItemProgressState.Normal;
                        eventAggregator.GetEvent<TransferStartedEvent>().Publish(new TransferStartedEventArgs(this));
                        _elapsedTimeMeter.Reset();
                        _elapsedTimeMeter.Start();
                        ProcessQueueItem();
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
                                var r = WindowManager.ShowIoErrorDialog(exception);
                                if (r != null)
                                {
                                    result = r;
                                    if (TargetPane != null) result.Action = TargetPane.IsResumeSupported ? CopyAction.Resume : CopyAction.Overwrite;
                                }
                            } 
                            else
                            {
                                WindowManager.ShowMessage(Resx.IOError, exception.Message);
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
                                var r = WindowManager.ShowWriteErrorDialog(sourceFile.Path, transferException.TargetFile, TargetPane.IsResumeSupported && sourceFile.Size > transferException.TargetFileSize, SourcePane, TargetPane);
                                if (r != null) result = r;
                            }
                        }
                        break;
                    case TransferErrorType.LostConnection:
                        var ftp = SourcePane as FtpContentViewModel ?? TargetPane as FtpContentViewModel;
                        if (WindowManager.ShowReconnectionDialog(exception) == true)
                        {
                            try
                            {
                                ftp.RestoreConnection();
                                ftp.Refresh();
                            } 
                            catch (Exception ex)
                            {
                                WindowManager.ShowMessage(Resx.ConnectionFailed, string.Format(Resx.CannotReestablishConnection, ex.Message));
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

        private void OpenTelnetSession(LocalFileSystemContentViewModel local, FtpContentViewModel ftp)
        {
            var connection = ftp.Connection;
            Telnet.OpenSession(local.Drive.Path, local.Drive.FullPath, connection.Address, connection.Port ?? 21, connection.Username, connection.Password);
        }

        private void CloseTelnetSession()
        {
            Telnet.CloseSession();
        }

        public void Pause()
        {
            IsPaused = true;
            SourcePane.Abort();
            TargetPane.Abort();
        }

        private void PauseMeters()
        {
            _speedMeter.Stop();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.Indeterminate;
        }

        public void Continue()
        {
            _elapsedTimeMeter.Start();
            ProgressState = TaskbarItemProgressState.Normal;
            IsPaused = false;
            _isContinued = true;
            ProcessQueueItem(CopyAction.Resume);
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
                UIThread.BeginRun(FinishTransfer);
            }
        }

        private void FinishTransfer()
        {
            _queue = null;
            _statistics.TimeSpentWithTransfer += _elapsedTimeMeter.Elapsed;
            if (_copyMode == CopyMode.RemoteExport || _copyMode == CopyMode.RemoteImport) CloseTelnetSession();
            SourcePane.FinishTransferAsSource();
            TargetPane.FinishTransferAsTarget();
            _speedMeter.Stop();
            _speedMeter.Reset();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.None;
            if (_sourceChanged) SourcePane.Refresh();
            if (_targetChanged) TargetPane.Refresh();
            eventAggregator.GetEvent<TransferFinishedEvent>().Publish(new TransferFinishedEventArgs(this));
        }

        private void RenameExistingFile(TransferException exception, CopyAction? action, Action<CopyAction?, string> rename, Action<Exception> chooseDifferentOption)
        {
            var name = WindowManager.ShowTextInputDialog(Resx.Rename, Resx.NewName + Strings.Colon, Path.GetFileName(exception.TargetFile));
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
            WorkerThread.Run(
                () =>
                {
                    Thread.Sleep(3000);
                    return true;
                },
                b =>
                {
                    if (finished) return;
                    WindowManager.ShowMessage(Resx.ApplicationIsBusy, Resx.PleaseWait, NotificationMessageFlags.NonClosable);
                });
            WorkerThread.Run(work,
                b =>
                {
                    WindowManager.CloseMessage();
                    finished = true;
                    success.Invoke(b);
                },
                e =>
                {
                    WindowManager.CloseMessage();
                    finished = true;
                    if (error != null) error.Invoke(e);
                });
        }

        private static void PopulationError(Exception ex)
        {
            ErrorMessage.Show(new SomethingWentWrongException(Resx.PopulationFailed, ex));
        }

    }
}