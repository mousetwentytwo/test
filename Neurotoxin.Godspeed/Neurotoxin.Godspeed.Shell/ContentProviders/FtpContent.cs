﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class FtpContent : TraceListener, IFileManager
    {
        private const char Slash = '/';
        char IFileManager.Slash
        {
            get { return Slash; }
        }

        private FtpConnection _connection;
        private string _connectionLostMessage;
        private readonly IEventAggregator _eventAggregator;
        private bool _isIdle = true;
        private bool _isAborted;

        public readonly Stack<string> Log = new Stack<string>();

        public string TempFilePath { get; set; }
        
        private readonly Timer _keepAliveTimer = new Timer(30000);
        public bool IsKeepAliveEnabled
        {
            get { return _keepAliveTimer.Enabled; }
            set { _keepAliveTimer.Enabled = value; }
        }

        public bool IsPlayStation3 { get; private set; }

        private FtpClient _ftpClient;
        private FtpClient FtpClient
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
            if (_isIdle) FtpClient.Execute("NOOP");
        }

        internal bool Connect(FtpConnection connection)
        {
            _ftpClient = new FtpClient
                {
                    EnableThreadSafeDataConnections = false,
                    DataConnectionType = connection.UsePassiveMode ? FtpDataConnectionType.PASV : FtpDataConnectionType.AutoActive,
                    Host = connection.Address, 
                    Port = connection.Port,
                    Credentials = string.IsNullOrEmpty(connection.Username)
                        ? new NetworkCredential(connection.Username, connection.Password)
                        : new NetworkCredential("anonymous", "no@email.com")
                };
            FtpTrace.AddListener(this);

            _connection = connection;
            _connectionLostMessage = string.Format("The connection with {0} has been lost.", connection.Name);
            _ftpClient.Connect();

            //TODO: doesn't work right now
            IsPlayStation3 = Log.Peek().Contains("220 multiMAN");

            var isAppendSupported = true;
            try
            {
                FtpClient.Execute("APPE");
            } 
            catch(FtpException ex)
            {
                //TODO: text might be different
                isAppendSupported = ex.Message != "command not recognized";
            }

            return isAppendSupported;
        }

        internal void Disconnect()
        {
            //TODO: remove tracelistener
            FtpClient.Dispose();
        }

        public List<FileSystemItem> GetDrives()
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var currentFolder = FtpClient.GetWorkingDirectory();
                FtpClient.SetWorkingDirectory("/");

                var result = FtpClient.GetListing()
                                       .Where(item => item.Name != "." && item.Name != "..")
                                       .Select(item => new FileSystemItem
                                           {
                                               Name = item.Name,
                                               Type = ItemType.Drive,
                                               Date = item.Modified,
                                               Path = string.Format("/{0}/", item.Name),
                                               FullPath = string.Format("{0}://{1}/", _connection.Name, item.Name),
                                               Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/drive.png")
                                           })
                                       .ToList();
                FtpClient.SetWorkingDirectory(currentFolder);
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            } 
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var currentPath = path;
                if (path != null) 
                    FtpClient.SetWorkingDirectory(path);
                else
                    currentPath = FtpClient.GetWorkingDirectory();
                if (!currentPath.EndsWith("/")) currentPath += "/";
                var result = FtpClient.GetListing()
                                       .Where(item => item.Name != "." && item.Name != "..")
                                       .Select(item => CreateModel(item, string.Format("{0}{1}{2}", currentPath, item.Name, item.Type == FtpFileSystemObjectType.Directory ? "/" : string.Empty)))
                                       .ToList();
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }          
        }

        private string LocateDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            FtpClient.SetWorkingDirectory(dir);
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
                var folder = FtpClient.GetListing().FirstOrDefault(item => item.Name == folderName);
                NotifyFtpOperationFinished();
                if (folder != null && folder.Type == FtpFileSystemObjectType.Directory)
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
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
        }

        public FileSystemItem GetFileInfo(string path)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                var file = FtpClient.GetListing().FirstOrDefault(item => item.Name == filename);
                NotifyFtpOperationFinished();
                return file != null && file.Type == FtpFileSystemObjectType.File ? CreateModel(file, path) : null;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
        }

        private FileSystemItem CreateModel(FtpListItem item, string path)
        {
            return new FileSystemItem
                       {
                           Name = item.Name,
                           //TODO: create mapper
                           Type = item.Type == FtpFileSystemObjectType.Directory ? ItemType.Directory : ItemType.File,
                           Date = item.Modified,
                           Path = path,
                           FullPath = string.Format("{0}:/{1}", _connection.Name, path),
                           Size = item.Type == FtpFileSystemObjectType.Directory ? null : (long?)item.Size
                       };
        }

        public DateTime GetFileModificationTime(string path)
        {
            try
            {
                return FtpClient.GetModifiedTime(path);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
        }

        public bool DriveIsReady(string drive)
        {
            try
            {
                FtpClient.SetWorkingDirectory(drive);
                return true;
            }
            catch (FtpException ex)
            {
                if (!FtpClient.IsConnected) throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>Unfortunately the FTP server in FSD2 doesn't support the FileExists command</remarks>
        /// <returns></returns>
        public bool FileExists(string path)
        {
            NotifyFtpOperationStarted(false);
            bool result;
            try
            {
                var filename = LocateDirectory(path);
                //TODO: Check FtpClient.FileExists();
                var list = FtpClient.GetListing();
                result = list.Any(file => file.Name == filename);
            }
            catch
            {
                result = false;
            }
            NotifyFtpOperationFinished();
            return result;
        }

        private long GetFileSize(string path)
        {
            var filename = LocateDirectory(path);
            var list = FtpClient.GetListing();
            return list.First(file => file.Name == filename).Size;
        }

        public byte[] ReadFileHeader(string path)
        {
            var ms = new MemoryStream(StfsPackage.DefaultHeaderSizeVersion1);
            DownloadFile(path, ms, 0, StfsPackage.DefaultHeaderSizeVersion1);
            var bytes = ms.ToArray();
            ms.Close();
            return bytes;
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
                if (fileSize == -1) fileSize = GetFileSize(remotePath);
                _isIdle = false;
                _isAborted = false;
                if (remoteStartPosition != 0) NotifyFtpOperationResumeStart(remoteStartPosition);

                long transferred = 0;
                using (var ftpStream = FtpClient.OpenRead(remotePath, remoteStartPosition))
                {
                    var buffer = new byte[0x8000];
                    int bufferSize;
                    while (!_isAborted && (bufferSize = ftpStream.Read(buffer, 0, 0x8000)) > 0)
                    {
                        fs.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyFtpOperationProgressChanged((int)(transferred * 100 /fileSize), bufferSize, transferred, remoteStartPosition);
                    }
                }
                fs.Flush();
                NotifyFtpOperationFinished(fs.Length);
            }
            catch (Exception ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
        }

        //internal void UploadFile(string remotePath, string localPath, bool append = false)
        //{
        //    NotifyFtpOperationStarted(true);
        //    try
        //    {
        //        _isIdle = false;
        //        var fi = new FileInfo(localPath);
        //        _fileSize = fi.Length;

        //        FtpClient.Upload(LocateDirectory(remotePath), localPath);
        //        NotifyFtpOperationFinished();
        //    }
        //    catch (Exception ex)
        //    {
        //        NotifyFtpOperationFinished();
        //        if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
        //        throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
        //    }
        //}

        internal void UploadFile(string remotePath, string localPath, bool append = false)
        {
            NotifyFtpOperationStarted(true);
            FileStream fs = null;
            try
            {
                _isIdle = false;
                long transferred = 0;
                var fileSize = new FileInfo(localPath).Length;
                fs = new FileStream(localPath, FileMode.Open);
                long resumeStartPosition = 0;
                if (append)
                {
                    transferred = GetFileSize(remotePath);
                    resumeStartPosition = transferred;
                    fs.Seek(transferred, SeekOrigin.Begin);
                    NotifyFtpOperationResumeStart(transferred);
                }
                using (var ftpStream = append ? FtpClient.OpenAppend(remotePath) : FtpClient.OpenWrite(remotePath))
                {
                    var buffer = new byte[0x8000];
                    int bufferSize;
                    while (!_isAborted && (bufferSize = fs.Read(buffer, 0, 0x8000)) > 0)
                    {
                        ftpStream.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyFtpOperationProgressChanged((int)(transferred * 100 / fileSize), bufferSize, transferred, resumeStartPosition);
                    }
                }
                NotifyFtpOperationFinished();
            }
            catch (IOException ex)
            {
                NotifyFtpOperationFinished();
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        public bool FolderExists(string path)
        {
            return FtpClient.DirectoryExists(path);
        }

        public void DeleteFolder(string path)
        {
            try
            {
                FtpClient.DeleteDirectory(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
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
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
            }
        }

        public void CreateFolder(string path)
        {
            try
            {
                FtpClient.CreateDirectory(path);
            }
            catch (FtpException ex)
            {
                //TODO: not read
                if (FtpClient.IsConnected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message, ex);
                throw new TransferException(TransferErrorType.LostConnection, _connectionLostMessage, ex);
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
            Connect(_connection);
        }

        private void NotifyFtpOperationStarted(bool binaryTransfer)
        {
            _eventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            _isIdle = true;
            _eventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
        }

        private void NotifyFtpOperationProgressChanged(int percentage, long transferred, long totalBytesTransferred, long resumeStartPosition)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, transferred, totalBytesTransferred, resumeStartPosition));
        }

        private void NotifyFtpOperationResumeStart(long resumeStartPosition)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(-1, resumeStartPosition, resumeStartPosition, resumeStartPosition));
        }

        public void Abort()
        {
            _isAborted = true;
        }

        #region Inherited TraceListener members

        public override void Write(string message)
        {
            if (!message.EndsWith(Environment.NewLine)) message = Log.Pop() + message;
            Log.Push(message);
        }

        public override void WriteLine(string message)
        {
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            Write(message);
        }

        #endregion
    }
}