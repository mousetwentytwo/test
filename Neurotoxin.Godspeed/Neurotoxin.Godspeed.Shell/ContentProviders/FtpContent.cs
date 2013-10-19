using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
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
        private string _connectionLostMessage;
        private readonly IEventAggregator _eventAggregator;
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;

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
            _ftpClient.Login(connection.Username, connection.Password);

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

                var result = _ftpClient.GetList().Select(item => GetFileInfo(item, string.Format("{0}/{1}{2}", currentPath, item.Name, item.IsFolder ? "/" : string.Empty))).ToList();
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
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            _ftpClient.ChangeFolder(dir);
            return filename;
        }

        public FileSystemItem GetFileInfo(string path)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                var file = _ftpClient.GetList().FirstOrDefault(item => item.Name == filename);
                NotifyFtpOperationFinished();
                return file != null && file.IsFile ? GetFileInfo(file, path) : null;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        private FileSystemItem GetFileInfo(FtpItem item, string path)
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

        public Stream GetFileStream(string path)
        {
            TempFilePath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), Guid.NewGuid());
            DownloadFile(path, TempFilePath, FileMode.Create);
            return new FileStream(TempFilePath, FileMode.Open);
        }

        public byte[] ReadFileContent(string path)
        {
            TempFilePath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), Guid.NewGuid());
            DownloadFile(path, TempFilePath, FileMode.Create);
            return File.ReadAllBytes(TempFilePath);
        }

        public byte[] ReadFileHeader(string path)
        {
            return DownloadHeader(path);
        }

        internal byte[] DownloadFile(string path)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                var result = _ftpClient.Download(LocateDirectory(path));
                NotifyFtpOperationFinished(result.Length);
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage);
            }
        }

        internal void DownloadFile(string remotePath, string localPath, FileMode mode, long remoteStartPosition = 0)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                var fs = new FileStream(localPath, mode);
                _ftpClient.Download(LocateDirectory(remotePath), remoteStartPosition, fs);
                fs.Flush();
                var length = fs.Length;
                fs.Close();
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
                _ftpClient.Upload(remotePath, localPath);
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
            NotifyFtpOperationProgressChanged((int)e.Percentage);
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
                _ftpClient.DeleteFolderRecursively(path);
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
            _eventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            _eventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
        }

        private void NotifyFtpOperationProgressChanged(int percentage)
        {
            _eventAggregator.GetEvent<FtpOperationProgressChangedEvent>().Publish(new FtpOperationProgressChangedEventArgs(percentage));
        }

        public void Abort()
        {
            _ftpClient.Abort();
        }
    }
}