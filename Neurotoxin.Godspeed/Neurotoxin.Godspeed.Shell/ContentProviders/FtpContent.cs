using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using Limilabs.FTP.Client;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class FtpContent : IFileManager
    {
        private const char Slash = '/';
        char IFileManager.Slash
        {
            get { return Slash; }
        }

        private FtpConnection _connection;
        private string _connectionLostMessage;
        private readonly IEventAggregator _eventAggregator;
        private bool _downloadHeaderOnly;
        private long _fileSize;
        private readonly Stopwatch _notificationTimer = new Stopwatch();
        private long _aggregatedTransferredValue;
        private long _resumeStartPosition;
        private bool _isIdle = true;

        public string TempFilePath { get; set; }
        
        private readonly Timer _keepAliveTimer = new Timer(30000);
        public bool IsKeepAliveEnabled
        {
            get { return _keepAliveTimer.Enabled; }
            set { _keepAliveTimer.Enabled = value; }
        }

        private Ftp _ftpClient;
        private Ftp FtpClient
        {
            get
            {
                if (_keepAliveTimer.Enabled)
                {
                    _keepAliveTimer.Stop();
                    _keepAliveTimer.Start();
                }
                return _ftpClient;
            }
        }

        public FtpContent(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _keepAliveTimer.AutoReset = true;
            _keepAliveTimer.Elapsed += KeepAliveTimerOnElapsed;
        }

        private void KeepAliveTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isIdle) FtpClient.Noop();
        }

        internal bool Connect(FtpConnection connection)
        {
            _ftpClient = new Ftp();
            _connection = connection;
            _connectionLostMessage = string.Format("The connection with {0} has been lost.", connection.Name);
            FtpClient.Connect(connection.Address, connection.Port);
            if (string.IsNullOrEmpty(connection.Username))
            {
                FtpClient.LoginAnonymous();
            }
            else
            {
                FtpClient.Login(connection.Username, connection.Password);
            }

            //HACK: FSD FTP states that it supports SIZE command, but it throws a "not implemented" exception
            var mi = FtpClient.Extensions.GetType().GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(FtpClient.Extensions, new object[] { false });

            var isAppendSupported = false;
            try
            {
                FtpClient.SendCommand("APPE");
            } 
            catch(FtpException ex)
            {
                isAppendSupported = ex.Message != "command not recognized";
            }

            FtpClient.Progress += FtpClientProgressChanged;

            return isAppendSupported;
        }

        internal void Disconnect()
        {
            try
            {
                FtpClient.Close();
            }
            catch
            {
                //NOTE: intentional
            }
            FtpClient.Dispose();
        }

        public List<FileSystemItem> GetDrives()
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var currentFolder = FtpClient.GetCurrentFolder();
                FtpClient.ChangeFolder("/");

                var result = FtpClient.GetList()
                                       .Where(item => item.Name != "." && item.Name != "..")
                                       .Select(item => new FileSystemItem
                                           {
                                               Name = item.Name,
                                               Type = ItemType.Drive,
                                               Date = item.ModifyDate,
                                               Path = string.Format("/{0}/", item.Name),
                                               FullPath = string.Format("{0}://{1}/", _connection.Name, item.Name),
                                               Thumbnail =
                                                   ApplicationExtensions.GetContentByteArray("/Resources/drive.png")
                                           })
                                       .ToList();
                FtpClient.ChangeFolder(currentFolder);
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            } 
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                if (path != null) FtpClient.ChangeFolder(path);
                var currentPath = FtpClient.GetCurrentFolder();
                if (!currentPath.EndsWith("/")) currentPath += "/";
                _fileSize = 0;

                var result = FtpClient.GetList()
                                       .Where(item => item.Name != "." && item.Name != "..")
                                       .Select(item => CreateModel(item, string.Format("{0}{1}{2}", currentPath, item.Name, item.IsFolder ? "/" : string.Empty)))
                                       .ToList();
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }          
        }

        private string LocateDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            FtpClient.ChangeFolder(dir);
            return filename;
        }

        public FileSystemItem GetFolderInfo(string path)
        {
            return GetFolderInfo(path, ItemType.Directory);
        }

        public FileSystemItem GetFolderInfo(string path, ItemType type)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var folderName = LocateDirectory(path);
                var folder = FtpClient.GetList().FirstOrDefault(item => item.Name == folderName);
                NotifyFtpOperationFinished();
                if (folder != null && folder.IsFolder)
                {
                    var m = CreateModel(folder, path);
                    m.Type = type;
                    return m;
                }
                return null;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public FileSystemItem GetFileInfo(string path)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                var file = FtpClient.GetList().FirstOrDefault(item => item.Name == filename);
                NotifyFtpOperationFinished();
                return file != null && file.IsFile ? CreateModel(file, path) : null;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        private FileSystemItem CreateModel(FtpItem item, string path)
        {
            return new FileSystemItem
                       {
                           Name = item.Name,
                           Type = item.IsFolder ? ItemType.Directory : ItemType.File,
                           Date = item.ModifyDate,
                           Path = path,
                           FullPath = string.Format("{0}:/{1}", _connection.Name, path),
                           Size = item.IsFolder ? null : item.Size
                       };
        }

        public DateTime GetFileModificationTime(string path)
        {
            try
            {
                return FtpClient.GetFileModificationTime(path);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public bool DriveIsReady(string drive)
        {
            try
            {
                FtpClient.ChangeFolder(drive);
                return true;
            }
            catch (FtpException)
            {
                if (!FtpClient.Connected) throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>Unfortunately the FTP server in FSD doesn't support the FileExists command</remarks>
        /// <returns></returns>
        public bool FileExists(string path)
        {
            NotifyFtpOperationStarted(false);
            bool result;
            try
            {
                var filename = LocateDirectory(path);
                var list = FtpClient.GetList();
                result = list.Any(file => file.Name == filename);
            }
            catch
            {
                result = false;
            }
            NotifyFtpOperationFinished();
            return result;
        }

        public byte[] ReadFileHeader(string path)
        {
            return DownloadHeader(path);
        }

        public byte[] ReadFileContent(string path, bool saveToTempFile = false, long fileSize = -1)
        {
            var ms = new MemoryStream();
            DownloadFile(path, ms, 0, fileSize);
            ms.Flush();
            var result = ms.ToArray();
            ms.Close();
            if (saveToTempFile)
            {
                TempFilePath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), Guid.NewGuid());
                File.WriteAllBytes(TempFilePath, result);
            }
            return result;
        }

        internal void DownloadFile(string remotePath, Stream fs, long remoteStartPosition = 0, long fileSize = -1)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                var filename = LocateDirectory(remotePath);
                if (fileSize == -1)
                {
                    var list = FtpClient.GetList();
                    fileSize = list.First(file => file.Name == filename).Size ?? 0;
                }
                _fileSize = fileSize;
                _isIdle = false;
                _resumeStartPosition = remoteStartPosition;
                if (remoteStartPosition != 0) NotifyFtpOperationResumeStart();
                FtpClient.Download(filename, remoteStartPosition, fs);
                var length = fs.Length;
                NotifyFtpOperationFinished(length);
            }
            catch (Exception ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        private byte[] DownloadHeader(string path)
        {
            NotifyFtpOperationStarted(false);
            _downloadHeaderOnly = true;
            var ms = new MemoryStream();
            try
            {
                FtpClient.Download(LocateDirectory(path), ms);
            }
            catch
            {
                //NOTE: this is intentional, unfortunately the ftp client will throw an exception after the Abort()
            }
            _downloadHeaderOnly = false;
            ms.Flush();
            RestoreConnection();
            NotifyFtpOperationFinished(ms.Length);
            var bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }

        internal void UploadFile(string remotePath, string localPath)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                _isIdle = false;
                var fi = new FileInfo(localPath);
                _fileSize = fi.Length;
                FtpClient.Upload(LocateDirectory(remotePath), localPath);
                NotifyFtpOperationFinished();
            }
            catch (Exception ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        internal void AppendFile(string remotePath, string localPath)
        {
            NotifyFtpOperationStarted(true);
            var filename = LocateDirectory(remotePath);
            var list = FtpClient.GetList();
            var file = list.FirstOrDefault(f => f.Name == filename);
            var size = file != null && file.Size.HasValue ? file.Size.Value : 0;
            _resumeStartPosition = size;
            FileStream fs = null;
            try
            {
                fs = new FileStream(localPath, FileMode.Open);
                fs.Seek(size, SeekOrigin.Begin);
                _isIdle = false;
                _fileSize = fs.Length;
                NotifyFtpOperationResumeStart();
                FtpClient.Append(remotePath, fs);
                NotifyFtpOperationFinished();
            }
            catch (IOException ex)
            {
                NotifyFtpOperationFinished();
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        private void FtpClientProgressChanged(object sender, ProgressEventArgs e)
        {
            var percentage = _fileSize != 0
                                 ? ((e.TotalBytesTransferred + _resumeStartPosition)*100/_fileSize)
                                 : e.Percentage;

            NotifyFtpOperationProgressChanged((int)percentage, e.Transferred, e.TotalBytesTransferred + _resumeStartPosition);

            if (_downloadHeaderOnly && e.TotalBytesTransferred > 0x971A) // v1 header size
                FtpClient.Abort();
        }

        public bool FolderExists(string path)
        {
            return FtpClient.FolderExists(path);
        }

        public void DeleteFolder(string path)
        {
            try
            {
                FtpClient.DeleteFolder(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public void DeleteFile(string path)
        {
            try
            {
                FtpClient.DeleteFile(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public void CreateFolder(string path)
        {
            try
            {
                FtpClient.CreateFolder(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (FtpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var oldName = LocateDirectory(path);
            var isFolder = FolderExists(path);
            FtpClient.Rename(oldName, newName);
            var r = new Regex(string.Format("{0}{1}?$", Regex.Escape(oldName), Slash), RegexOptions.IgnoreCase);
            path = r.Replace(path, newName);
            return isFolder ? GetFolderInfo(path + Slash) : GetFileInfo(path);
        }

        public void RestoreConnection()
        {
            FtpClient.Progress -= FtpClientProgressChanged;
            Connect(_connection);
        }

        private void NotifyFtpOperationStarted(bool binaryTransfer)
        {
            _notificationTimer.Reset();
            _notificationTimer.Start();
            _eventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            _isIdle = true;
            _notificationTimer.Stop();
            _eventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
        }

        private void NotifyFtpOperationProgressChanged(int percentage, long transferred, long totalBytesTransferred)
        {
            _aggregatedTransferredValue += transferred;
            if (_notificationTimer.Elapsed.TotalMilliseconds < 100 && percentage != 100) return;

            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, _aggregatedTransferredValue, totalBytesTransferred, _resumeStartPosition));
            _notificationTimer.Restart();
            _aggregatedTransferredValue = 0;
        }

        private void NotifyFtpOperationResumeStart()
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(-1, _resumeStartPosition, _resumeStartPosition, _resumeStartPosition));
        }

        public void Abort()
        {
            FtpClient.Abort();
        }
    }
}