using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();

        public FtpConnectionItemViewModel Connection { get; private set; }

        public Stack<string> Log { get { return FileManager.Log; } } 

        public bool IsKeepAliveEnabled
        {
            get { return FileManager.IsKeepAliveEnabled; }
            set { FileManager.IsKeepAliveEnabled = value; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }
        
        protected override string ExportActionDescription
        {
            get { return "Download"; }
        }

        protected override string ImportActionDescription
        {
            get { return "Upload"; }
        }

        private string ConnectionLostMessage
        {
            get
            {
                return string.Format("The connection with {0} has been lost.", Connection.Name);
            }
        }

        #region Commands

        private void ExecuteCloseCommand()
        {
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, Connection));
            Dispose();
        }

        protected override bool CanExecuteOpenCompressedFileCommand()
        {
            return false;
        }

        #endregion

        public FtpContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                            {
                                Connection = (FtpConnectionItemViewModel)cmdParam.Payload;
                                return Connect();
                            },
                        result =>
                            {
                                IsLoaded = true;
                                IsResumeSupported = result;
                                try
                                {
                                    ConnectCallback();
                                } 
                                catch(Exception ex)
                                {
                                    if (error != null)
                                    {
                                        var somethingWentWrong = string.Format("Something went wrong while trying to establish connection. Please try again, and if the error persists try to turn {0} Passive Mode.", Connection.UsePassiveMode ? "off" : "on");
                                        error.Invoke(this, new SomethingWentWrongException(somethingWentWrong, ex));
                                    }
                                    CloseCommand.Execute();
                                    return;
                                }
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam.Payload as BinaryContent;
                    if (payload == null) return;
                    WorkerThread.Run(
                        () =>
                            {
                                File.WriteAllBytes(payload.TempFilePath, payload.Content);
                                FileManager.RestoreConnection();
                                FileManager.UploadFile(payload.FilePath, payload.TempFilePath);
                                return true;
                            },
                        result =>
                            {
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                CloseCommand.Execute();
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
            }
        }

        private bool Connect()
        {
            return FileManager.Connect(Connection.Model);
        }

        private void ConnectCallback()
        {
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
            var r = new Regex("^/[A-Z0-9_-]+/", RegexOptions.IgnoreCase);
            var defaultPath = string.IsNullOrEmpty(Connection.Model.DefaultPath)
                                  ? FileManager.ServerType == FtpServerType.PlayStation3 ? "/dev_hdd0/" : "/Hdd1/"
                                  : Connection.Model.DefaultPath;

            var m = r.Match(defaultPath);
            FileSystemItemViewModel drive = null;
            if (m.Success)
            {
                drive = Drives.SingleOrDefault(d => d.Path == m.Value);
                if (drive != null && FileManager.FolderExists(defaultPath)) PathCache.Add(drive, defaultPath);
            }
            Drive = drive ?? Drives.First();
        }

        public void RestoreConnection()
        {
            FileManager.RestoreConnection();
        }

        public override void Abort()
        {
            FileManager.Abort();
        }

        protected override void ChangeDrive()
        {
            if (!_driveLabelCache.ContainsKey(Drive.Path))
            {
                var path = String.Format("{0}name.txt", Drive.Path);
                string label = null;
                if (FileManager.FileExists(path))
                {
                    var bytes = FileManager.ReadFileContent(path);
                    label = String.Format("[{0}]", Encoding.BigEndianUnicode.GetString(bytes));
                }
                _driveLabelCache.Add(Drive.Path, label);
            }
            DriveLabel = _driveLabelCache[Drive.Path];
            base.ChangeDrive();
        }

        protected override List<FileSystemItem> ChangeDirectoryInner(string selectedPath)
        {
            return FileManager.ServerType == FtpServerType.PlayStation3 ? FileManager.GetList(selectedPath) : base.ChangeDirectoryInner(selectedPath);
        }

        protected override void ChangeDirectoryCallback(List<FileSystemItem> result)
        {
            base.ChangeDirectoryCallback(result);
            if (FileManager.ServerType != FtpServerType.PlayStation3) return;

            //PS3 Transfer complete response string
            var r = new Regex(string.Format(@"226 Transfer complete \[{0}\] \[ ([0-9]+.*?free) \]", Drive.Path.TrimEnd('/')));
            var m = r.Match(FileManager.Log.ElementAt(1));
            if (m.Success) FreeSpace = m.Groups[1].Value;
        }

        public override string GetTargetPath(string path)
        {
            return String.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override void SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.DownloadFile(item.Path, fs, remoteStartPosition, item.Size ?? 0);
        }

        protected override Exception WrapTransferRelatedExceptions(Exception exception)
        {
            if (exception is IOException || exception is FtpException)
            {
                return FileManager.IsConnected
                           ? new TransferException(TransferErrorType.NotSpecified, exception.Message, exception)
                           : new TransferException(TransferErrorType.LostConnection, ConnectionLostMessage, exception);
            }
            return base.WrapTransferRelatedExceptions(exception);
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            FileManager.UploadFile(targetPath, sourcePath);
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            FileManager.UploadFile(targetPath, sourcePath);
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
        {
            //FileManager.AppendFile(targetPath, sourcePath);
            FileManager.UploadFile(targetPath, sourcePath, true);
        }

        //protected override string OpenCompressedFile(FileSystemItem item)
        //{
        //    var tempFilePath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), Guid.NewGuid());
        //    var fs = new FileStream(tempFilePath, FileMode.Create);
        //    FileManager.DownloadFile(item.Path, fs, 0, item.Size ?? 0);
        //    return tempFilePath;
        //}

        public bool RemoteDownload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            eventAggregator.GetEvent<TransferActionStartedEvent>().Publish(ExportActionDescription);
            long resumeStartPosition = 0;
            try
            {
                switch (action)
                {
                    case CopyAction.CreateNew:
                        if (File.Exists(savePath))
                            throw new TransferException(TransferErrorType.WriteAccessError, item.Path, savePath, "Target already exists");
                        break;
                    case CopyAction.Overwrite:
                        File.Delete(savePath);
                        break;
                    case CopyAction.OverwriteOlder:
                        var fileDate = File.GetLastWriteTime(savePath);
                        if (fileDate > item.Date) return false;
                        File.Delete(savePath);
                        break;
                    case CopyAction.Resume:
                        var fi = new FileInfo(savePath);
                        resumeStartPosition = fi.Length;
                        break;
                }

                var name = RemoteChangeDirectory(item.Path);
                Telnet.Download(name, savePath, item.Size ?? 0, resumeStartPosition, TelnetProgressChanged);
                return true;
            }
            catch (Exception ex)
            {
                throw WrapTransferRelatedExceptions(ex);
            }
        }

        public bool RemoteUpload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            eventAggregator.GetEvent<TransferActionStartedEvent>().Publish(ImportActionDescription);
            long resumeStartPosition = 0;
            try
            {
                switch (action)
                {
                    case CopyAction.CreateNew:
                        var exists = FileManager.FileExists(savePath);
                        if (exists) throw new TransferException(TransferErrorType.WriteAccessError, "Target already exists", item.Path, savePath, exists.Size);
                        break;
                    case CopyAction.Overwrite:
                        FileManager.DeleteFile(savePath);
                        break;
                    case CopyAction.OverwriteOlder:
                        var fileDate = FileManager.GetFileModificationTime(savePath);
                        if (fileDate > item.Date) return false;
                        FileManager.DeleteFile(savePath);
                        break;
                    case CopyAction.Resume:
                        var fi = FileManager.GetItemInfo(savePath, ItemType.File);
                        resumeStartPosition = fi.Size ?? 0;
                        break;
                }
                var name = RemoteChangeDirectory(savePath);
                Telnet.Upload(item.Path, name, item.Size ?? 0, resumeStartPosition, TelnetProgressChanged);
                return true;
            }
            catch (Exception ex)
            {
                throw WrapTransferRelatedExceptions(ex);
            }
        }

        private void TelnetProgressChanged(int p, long t, long total, long resumeStartPosition)
        {
            var args = new TransferProgressChangedEventArgs(p, t, total, resumeStartPosition);
            eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(args);
        }

        private string RemoteChangeDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            Telnet.ChangeFtpDirectory(dir);
            return path.Replace(dir, string.Empty);
        }

        public override void Dispose()
        {
            FileManager.Disconnect();
            if (CurrentFolder != null) Connection.Model.DefaultPath = CurrentFolder.Path;
            base.Dispose();
        }

        public override object Close(object data)
        {
            Dispose();
            return Connection;
        }
    }
}