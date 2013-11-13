using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private const char SLASH = '/';
        public char Slash
        {
            get { return SLASH; }
        }
        
        private string _connectionLostMessage;
        private readonly IEventAggregator _eventAggregator;
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;
        private long _downloadFileSize;
        private FtpTransferDirection _transferDirection;
        private readonly Stopwatch _notificationTimer = new Stopwatch();
        private long _aggregatedTransferredValue;

        public string TempFilePath { get; set; }
        public FtpConnection Connection { get; private set; }

        public FtpContent(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        internal void Connect(FtpConnection connection)
        {
            _ftpClient = new Ftp();
            _ftpClient.Connect(connection.Address, connection.Port);
            if (string.IsNullOrEmpty(connection.Username))
            {
                _ftpClient.LoginAnonymous();
            }
            else
            {
                _ftpClient.Login(connection.Username, connection.Password);
            }

            //HACK: FSD FTP states that it supports SIZE command, but it throws a "not implemented" exception
            var mi = _ftpClient.Extensions.GetType().GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(_ftpClient.Extensions, new object[] { false });

            _ftpClient.Progress += FtpClientProgressChanged;
            Connection = connection;
            _connectionLostMessage = string.Format("The connection with {0} has been lost.", connection.Name);
        }

        internal void Disconnect()
        {
            try
            {
                _ftpClient.Close();
            }
            catch
            {
                //NOTE: intentional
            }
            _ftpClient.Dispose();
        }

        public List<FileSystemItem> GetDrives()
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var currentFolder = _ftpClient.GetCurrentFolder();
                _ftpClient.ChangeFolder("/");

                var result = _ftpClient.GetList().Select(di => new FileSystemItem
                    {
                        Name = di.Name,
                        Type = ItemType.Drive,
                        Date = di.ModifyDate,
                        Path = string.Format("/{0}/", di.Name),
                        FullPath = string.Format("{0}://{1}/", Connection.Name, di.Name),
                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/drive.png")
                    }).ToList();
                _ftpClient.ChangeFolder(currentFolder);
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            } 
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                if (path != null) _ftpClient.ChangeFolder(path);
                var currentPath = _ftpClient.GetCurrentFolder();

                var items = _ftpClient.GetList();
                var result = items.Select(item => CreateModel(item, string.Format("{0}/{1}{2}", currentPath, item.Name, item.IsFolder ? "/" : string.Empty))).ToList();
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }          
        }

        private string LocateDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            _ftpClient.ChangeFolder(dir);
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
                var folder = _ftpClient.GetList().FirstOrDefault(item => item.Name == folderName);
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
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public FileSystemItem GetFileInfo(string path)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                var file = _ftpClient.GetList().FirstOrDefault(item => item.Name == filename);
                NotifyFtpOperationFinished();
                return file != null && file.IsFile ? CreateModel(file, path) : null;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
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
                           FullPath = string.Format("{0}:/{1}", Connection.Name, path),
                           Size = item.IsFolder ? null : item.Size
                       };
        }

        public DateTime GetFileModificationTime(string path)
        {
            try
            {
                return _ftpClient.GetFileModificationTime(path);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public bool DriveIsReady(string drive)
        {
            try
            {
                _ftpClient.ChangeFolder(drive);
                return true;
            }
            catch (FtpException)
            {
                if (!_ftpClient.Connected) throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
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
                var list = _ftpClient.GetList();
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
                    var list = _ftpClient.GetList();
                    fileSize = list.First(file => file.Name == filename).Size ?? 0;
                }
                _transferDirection = FtpTransferDirection.Download;
                _downloadFileSize = fileSize;
                _ftpClient.Download(filename, remoteStartPosition, fs);
                var length = fs.Length;
                NotifyFtpOperationFinished(length);
            }
            catch (Exception ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
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
                _ftpClient.Download(LocateDirectory(path), ms);
            }
            catch
            {
                //NOTE: this is intentional, unfortunately the ftp client will throw an exception after the Abort()
            }
            _downloadHeaderOnly = false;
            ms.Flush();
            RestoreConnection();
            NotifyFtpOperationFinished(ms.Length);
            return ms.ToArray();
        }

        internal void UploadFile(string remotePath, string localPath)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                _transferDirection = FtpTransferDirection.Upload;
                _ftpClient.Upload(LocateDirectory(remotePath), localPath);
                NotifyFtpOperationFinished();
            }
            catch (Exception ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        internal void AppendFile(string remotePath, string localPath)
        {
            NotifyFtpOperationStarted(true);
            var filename = LocateDirectory(remotePath);
            var list = _ftpClient.GetList();
            var file = list.FirstOrDefault(f => f.Name == filename);
            var size = file != null ? file.Size.Value : 0;
            try
            {
                var fs = new FileStream(localPath, FileMode.Open);
                fs.Seek(size, SeekOrigin.Begin);
                _transferDirection = FtpTransferDirection.Upload;
                _ftpClient.Append(remotePath, fs);
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
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.WriteAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        private void FtpClientProgressChanged(object sender, ProgressEventArgs e)
        {
            var percentage = _transferDirection == FtpTransferDirection.Download && _downloadFileSize != 0
                                 ? (e.TotalBytesTransferred*100/_downloadFileSize)
                                 : e.Percentage;

            NotifyFtpOperationProgressChanged((int) percentage, e.Transferred, e.TotalBytesTransferred);

            if (_downloadHeaderOnly && e.TotalBytesTransferred > 0x971A) // v1 header size
                _ftpClient.Abort();
        }

        public bool FolderExists(string path)
        {
            return _ftpClient.FolderExists(path);
        }

        public void DeleteFolder(string path)
        {
            try
            {
                _ftpClient.DeleteFolder(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public void DeleteFile(string path)
        {
            try
            {
                _ftpClient.DeleteFile(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public void CreateFolder(string path)
        {
            try
            {
                _ftpClient.CreateFolder(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        public void RestoreConnection()
        {
            _ftpClient.Progress -= FtpClientProgressChanged;
            Connect(Connection);
        }

        private void NotifyFtpOperationStarted(bool binaryTransfer)
        {
            _notificationTimer.Reset();
            _notificationTimer.Start();
            _eventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            _notificationTimer.Stop();
            _eventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
        }

        private void NotifyFtpOperationProgressChanged(int percentage, long transferred, long totalBytesTransferred)
        {
            _aggregatedTransferredValue += transferred;
            if (_notificationTimer.Elapsed.TotalMilliseconds < 100 && percentage != 100) return;

            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, _aggregatedTransferredValue, totalBytesTransferred));
            _notificationTimer.Restart();
            _aggregatedTransferredValue = 0;
        }

        public void Abort()
        {
            _ftpClient.Abort();
        }
    }
}