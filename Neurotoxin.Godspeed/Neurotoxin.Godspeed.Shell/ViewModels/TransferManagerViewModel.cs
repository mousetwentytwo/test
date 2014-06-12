using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
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
        private Shutdown _shutdown;

        private readonly IStatisticsViewModel _statistics;
        private readonly IUserSettings _userSettings;

        #region Properties

        public IFileListPaneViewModel SourcePane { get; private set; }
        public IFileListPaneViewModel TargetPane { get; private set; }

        private FtpContentViewModel Ftp
        {
            get
            {
                return SourcePane as FtpContentViewModel ?? TargetPane as FtpContentViewModel;
            }
        }

        public string XboxName
        {
            get { return Ftp != null ? Ftp.Connection.Name : "Xbox"; }
        }

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

        public bool IsVerificationSupported
        {
            get { return Ftp != null && Ftp.IsFSD; }
        }

        public bool IsShutdownSupported
        {
            get { return Ftp != null && Ftp.IsFSD; }
        }

        private const string ISVERIFICATIONENABLED = "IsVerificationEnabled";
        public bool IsVerificationEnabled
        {
            get { return _userSettings.VerifyFileHashAfterFtpUpload; }
            set { _userSettings.VerifyFileHashAfterFtpUpload = value; NotifyPropertyChanged(ISVERIFICATIONENABLED); }
        }

        private const string ISSHUTDOWNPCENABLED = "IsShutdownPcEnabled";
        public bool IsShutdownPcEnabled
        {
            get { return _shutdown == Shutdown.PC; }
            set { EnumHelper.SetFlag(_shutdown, Shutdown.PC, value); NotifyPropertyChanged(ISSHUTDOWNPCENABLED); }
        }

        private const string ISSHUTDOWNXBOXENABLED = "IsShutdownXboxEnabled";
        public bool IsShutdownXboxEnabled
        {
            get { return _shutdown == Shutdown.Xbox; }
            set { EnumHelper.SetFlag(_shutdown, Shutdown.Xbox, value); NotifyPropertyChanged(ISSHUTDOWNXBOXENABLED); }
        }

        private const string ISCOMPLETESHUTDOWNENABLED = "IsCompleteShutdownEnabled";
        public bool IsCompleteShutdownEnabled
        {
            get { return _shutdown == Shutdown.Both; }
            set { EnumHelper.SetFlag(_shutdown, Shutdown.Both, value); NotifyPropertyChanged(ISCOMPLETESHUTDOWNENABLED); }
        }

        #endregion

        public TransferManagerViewModel(IUserSettings userSettings, IStatisticsViewModel statistics)
        {
            _userSettings = userSettings;
            _statistics = statistics;
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
            EventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Subscribe(OnShowCorrespondingError);
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

                    FileMode mode;
                    FileExistenceInfo exists;
                    long startPosition = 0;
                    switch (a)
                    {
                        case CopyAction.CreateNew:
                            //TODO: check what happens if targetPath contains spec.char
                            exists = TargetPane.FileExists(targetPath);
                            if (exists) throw new TransferException(TransferErrorType.WriteAccessError, Resx.TargetAlreadyExists)
                                                  {
                                                      SourceFile = item.Path, 
                                                      TargetFile = targetPath, 
                                                      TargetFileSize = exists.Size
                                                  };
                            mode = FileMode.CreateNew;
                            break;
                        case CopyAction.Overwrite:
                            mode = FileMode.Create;
                            break;
                        case CopyAction.OverwriteOlder:
                            var fileDate = File.GetLastWriteTime(targetPath);
                            if (fileDate > item.Date) return new OperationResult(TransferResult.Skipped, targetPath);
                            mode = FileMode.Create;
                            break;
                        case CopyAction.Resume:
                            mode = FileMode.Append;
                            exists = TargetPane.FileExists(targetPath);
                            if (exists) startPosition = exists.Size;
                            break;
                        default:
                            throw new ArgumentException("Invalid Copy action: " + action);
                    }

                    //TODO: remote

                    using (var targetStream = TargetPane.GetStream(targetPath, mode, FileAccess.Write, startPosition))
                    {
                        UIThread.Run(() => { TransferAction = GetCopyActionText(); });
                        result = SourcePane.CopyStream(item, targetStream, startPosition) ? TransferResult.Ok : TransferResult.Aborted;
                    }

                    //switch (_copyMode)
                    //{
                    //    case CopyMode.DirectExport:
                    //        result = SourcePane.Export(item, targetPath, a);
                    //        break;
                    //    case CopyMode.DirectImport:
                    //        result = TargetPane.Import(item, targetPath, a);
                    //        break;
                    //    case CopyMode.Indirect:
                    //        var tempFile = Path.Combine(App.DataDirectory, "temp", item.FullPath.Hash());
                    //        var tempItem = item.Clone();
                    //        tempItem.Path = tempFile;
                    //        var export = SourcePane.Export(item, tempFile, CopyAction.Overwrite);
                    //        var import = TransferResult.Skipped;
                    //        if (export == TransferResult.Ok)
                    //            import = TargetPane.Import(tempItem, targetPath, action ?? _rememberedCopyAction);
                    //        File.Delete(tempFile);
                    //        result = export != TransferResult.Ok ? export : import;
                    //        break;
                    //    case CopyMode.RemoteExport:
                    //        if (new Regex(@"[^\x20-\x7f]").IsMatch(targetPath))
                    //        {
                    //            //TODO: not sure this is the right idea
                    //            _copyMode = CopyMode.DirectExport;
                    //            CloseTelnetSession();
                    //            EventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                    //            result = SourcePane.Export(item, targetPath, a);
                    //        } 
                    //        else
                    //        {
                    //            result = ((FtpContentViewModel)SourcePane).RemoteDownload(item, targetPath, a);
                    //        }
                    //        break;
                    //    case CopyMode.RemoteImport:
                    //        if (new Regex(@"[^\x20-\x7f]").IsMatch(item.Path))
                    //        {
                    //            //TODO: not sure this is the right idea
                    //            _copyMode = CopyMode.DirectImport;
                    //            CloseTelnetSession();
                    //            EventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs("RemoteCopySpecialCharsWarningMessage", MessageIcon.Info));
                    //            result = TargetPane.Import(item, targetPath, a);
                    //        } else
                    //        {
                    //            result = ((FtpContentViewModel)TargetPane).RemoteUpload(item, targetPath, a);
                    //        }
                    //        break;
                    //    default:
                    //        throw new NotSupportedException("Invalid Copy Mode: " + _copyMode);
                    //}
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
            UIThread.Run(() => TransferAction = Resx.Verifying);
            string remotePath;
            string localPath;
            if (Ftp == TargetPane)
            {
                remotePath = (string) item.Payload;
                localPath = item.FileSystemItem.Path;
            } 
            else if (Ftp == SourcePane)
            {
                localPath = (string)item.Payload;
                remotePath = item.FileSystemItem.Path;
            }
            else
            {
                throw new ApplicationException();
            }
            var verificationResult = Ftp.Verify(remotePath, localPath);
            return new OperationResult(verificationResult);
        }

        private void ProcessQueueItem(CopyAction? action = null, string rename = null)
        {
            if (_queue.Count > 0)
            {
                var queueitem = _queue.Peek();
                var item = queueitem.FileSystemItem;
                SourceFile = string.IsNullOrEmpty(SourcePane.CurrentFolder.Path) ? item.Path : item.Path.Replace(SourcePane.CurrentFolder.Path, string.Empty);
                WorkHandler.Run(() =>
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
                    if (queueItem.Operation == FileOperation.Copy) _targetChanged = true;
                    if (queueItem.Operation == FileOperation.Copy && IsVerificationSupported && IsVerificationEnabled && queueItem.FileSystemItem.Type == ItemType.File)
                    {
                        queueItem.Operation = FileOperation.Verify;
                        queueItem.Payload = result.TargetPath;
                    } 
                    else if ((queueItem.Operation == FileOperation.Copy || queueItem.Operation == FileOperation.Verify) && UserAction == FileOperation.Move)
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
                    _queue.Dequeue();
                    break;
                case TransferResult.Aborted:
                    if (queueItem.Operation == FileOperation.Copy) _targetChanged = true;
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


            if (Ftp != null && !Ftp.IsConnected)
            {
                exception = new TransferException(TransferErrorType.LostConnection, string.Format(Resx.ConnectionLostMessage, Ftp.Connection.Name), exception)
                                {
                                    Pane = Ftp
                                };
            }

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

        private string GetCopyActionText()
        {
            if (TargetPane is FtpContentViewModel) return Resx.Upload;
            if (SourcePane is FtpContentViewModel) return Resx.Download;
            if (TargetPane is StfsPackageContentViewModel || TargetPane is CompressedFileContentViewModel) return Resx.Inject;
            if (SourcePane is StfsPackageContentViewModel || SourcePane is CompressedFileContentViewModel) return Resx.Extract;
            return Resx.ResourceManager.GetString(UserAction.ToString());
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
                        _statistics.BytesTransferred += args.TotalBytesTransferred;
                    } 
                    else if (!_speedMeter.IsRunning)
                    {
                        _speedMeter.Restart();
                    }
                    var ms = _speedMeter.Elapsed.TotalMilliseconds;
                    if (ms > 100) Speed = (int)Math.Floor((args.TotalBytesTransferred)/ms*1000/1024);
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
            TransferAction = mode == FileOperation.Copy ? GetCopyActionText() : Resx.ResourceManager.GetString(mode.ToString());
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
            WorkHandler.Run(
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
                        EventAggregator.GetEvent<TransferStartedEvent>().Publish(new TransferStartedEventArgs(this));
                        _elapsedTimeMeter.Reset();
                        _elapsedTimeMeter.Start();
                        ProcessQueueItem();
                    });
        }

        internal TransferErrorDialogResult ShowCorrespondingErrorDialog(Exception exception, bool feedbackNeeded = true)
        {
            var transferException = exception as TransferException;
            var exceptionType = transferException != null ? transferException.Type : TransferErrorType.NotSpecified;

            _elapsedTimeMeter.Stop();
            
            var result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
            switch (exceptionType)
            {
                case TransferErrorType.NotSpecified:
                    {
                        if (feedbackNeeded)
                        {
                            var r = WindowManager.ShowIoErrorDialog(exception);
                            if (r != null)
                            {
                                result = r;
                                //if (transferException.TargetPane != null) result.Action = transferException.TargetPane.IsResumeSupported ? CopyAction.Resume : CopyAction.Overwrite;
                            }
                        } 
                        else
                        {
                            WindowManager.ShowMessage(Resx.IOError, exception.Message);
                        }
                    }
                    break;
                case TransferErrorType.WriteAccessError:
                case TransferErrorType.NotSupporterCharactersInPath:
                    {
                        if (_skipAll != null)
                        {
                            result = _skipAll;
                        }
                        else
                        {
                            var sourceFile = _queue.Peek().FileSystemItem;
                            var flags = CopyAction.Rename;
                            if (TargetPane.IsResumeSupported && sourceFile.Size > transferException.TargetFileSize) flags |= CopyAction.Resume;
                            if (exceptionType == TransferErrorType.WriteAccessError) flags = flags | CopyAction.Overwrite | CopyAction.OverwriteOlder;
                            var sourcePath = sourceFile.Path;
                            var targetPath = transferException.TargetFile;
                            var r = WindowManager.ShowWriteErrorDialog(sourcePath, targetPath, flags, () =>
                                                                                                          {
                                                                                                              SourcePane.GetItemViewModel(sourcePath);
                                                                                                              TargetPane.GetItemViewModel(targetPath);
                                                                                                          });
                            if (r != null) result = r;
                        }
                    }
                    break;
                case TransferErrorType.LostConnection:
                    var ftp = (FtpContentViewModel)transferException.Pane;
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
                    throw new NotSupportedException("Invalid transfer error type: " + exceptionType);
            }

            //TODO: refactor scoping
            if (result.Scope == CopyActionScope.All)
            {
                if (result.Action.HasValue) _rememberedCopyAction = result.Action.Value;
                if (result.Behavior == ErrorResolutionBehavior.Skip) _skipAll = result;
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
                UIThread.Run(FinishTransfer);
            }
        }

        private void FinishTransfer()
        {
            _queue = null;
            _statistics.TimeSpentWithTransfer += _elapsedTimeMeter.Elapsed;
            if (_copyMode == CopyMode.RemoteExport || _copyMode == CopyMode.RemoteImport) CloseTelnetSession();
            SourcePane.FinishTransferAsSource();
            if (TargetPane != null) TargetPane.FinishTransferAsTarget();
            _speedMeter.Stop();
            _speedMeter.Reset();
            _elapsedTimeMeter.Stop();
            ProgressState = TaskbarItemProgressState.None;

            var args = new TransferFinishedEventArgs(this);

            if (_shutdown.HasFlag(Shutdown.Xbox))
            {
                if (Ftp == SourcePane) _sourceChanged = false;
                if (Ftp == TargetPane) _targetChanged = false;
                Ftp.Shutdown();
            }
            if (_shutdown.HasFlag(Shutdown.PC)) args.Shutdown = true;

            if (_sourceChanged) SourcePane.Refresh();
            if (_targetChanged) TargetPane.Refresh();
            SourcePane = null;
            TargetPane = null;
            EventAggregator.GetEvent<TransferFinishedEvent>().Publish(args);
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
            WorkHandler.Run(
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
            WorkHandler.Run(work,
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

        private void PopulationError(Exception ex)
        {
            WindowManager.ShowErrorMessage(new SomethingWentWrongException(Resx.PopulationFailed, ex));
        }

    }
}