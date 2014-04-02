using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

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
        private readonly IEventAggregator _eventAggregator;
        private bool _isIdle = true;
        private bool _isAborted;
        private readonly Stopwatch _notificationTimer = new Stopwatch();
        private long _aggregatedTransferredValue;

        public readonly Stack<string> Log = new Stack<string>();

        public string TempFilePath { get; set; }
        
        private readonly Timer _keepAliveTimer = new Timer(30000);
        public bool IsKeepAliveEnabled
        {
            get { return _keepAliveTimer.Enabled; }
            set { _keepAliveTimer.Enabled = value; }
        }

        public FtpServerType ServerType { get; private set; }

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

        public bool IsConnected
        {
            get { return FtpClient.IsConnected; }
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
                    Credentials = !string.IsNullOrEmpty(connection.Username)
                        ? new NetworkCredential(connection.Username, connection.Password)
                        : new NetworkCredential("anonymous", "no@email.com")
                };
            FtpTrace.AddListener(this);

            _connection = connection;
            _ftpClient.Connect();

            lock (Log)
            {
                foreach (var log in Log.Where(e => e.StartsWith("220")))
                {
                    FtpServerType type;
                    if (EnumHelper.TryGetField(log.Trim(), out type))
                    {
                        ServerType = type;
                        break;
                    }
                }
            }

            var r = FtpClient.Execute("SIZE");
            if (r.Message.Contains("command not recognized") || r.Message.Contains("command not implemented"))
            {
                _ftpClient.Capabilities &= ~FtpCapability.SIZE;
            }

            r = FtpClient.Execute("APPE");
            return !r.Message.Contains("command not recognized");
        }

        internal void Disconnect()
        {
            FtpTrace.RemoveListener(this);
            FtpClient.Dispose();
        }

        public List<FileSystemItem> GetDrives()
        {
            List<FileSystemItem> result;
            NotifyFtpOperationStarted(false);
            try
            {
                var currentFolder = FtpClient.GetWorkingDirectory();
                FtpClient.SetWorkingDirectory("/");

                result = FtpClient.GetListing()
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
            }
            finally
            {
                NotifyFtpOperationFinished();
            }
            return result;
        }

        public List<FileSystemItem> GetList(string path = null)
        {

            List<FileSystemItem> result;
            NotifyFtpOperationStarted(false);
            try
            {
                var currentPath = path;
                if (path != null)
                    FtpClient.SetWorkingDirectory(path);
                else
                    currentPath = FtpClient.GetWorkingDirectory();
                if (!currentPath.EndsWith("/")) currentPath += "/";
                try
                {
                    result = FtpClient.GetListing()
                                      .Select(item => CreateModel(item, string.Format("{0}{1}{2}", currentPath, item.Name, item.Type == FtpFileSystemObjectType.Directory ? "/" : string.Empty)))
                                      .ToList();
                }
                catch (Exception ex)
                {
                    //XeXMenu throws an exception if the folder is empty (dull)
                    if (ServerType == FtpServerType.XeXMenu && ex.Message.Contains("Path not found")) return new List<FileSystemItem>();
                    throw;
                }
            }
            finally
            {
                NotifyFtpOperationFinished();
            }
            return result;
        }

        private string LocateDirectory(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            FtpClient.SetWorkingDirectory(dir);
            return filename;
        }

        public FileSystemItem GetItemInfo(string path)
        {
            return GetItemInfo(path, null, true);
        }

        public FileSystemItem GetItemInfo(string path, ItemType? type)
        {
            return GetItemInfo(path, type, true);
        }

        public FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var itemName = LocateDirectory(path);
                var item = FtpClient.GetListing().FirstOrDefault(i => i.Name == itemName);
                NotifyFtpOperationFinished();
                if (item == null) return null;
                if (type != null)
                {
                    FtpFileSystemObjectType ftpItemType;
                    switch (type)
                    {
                        case ItemType.File:
                            ftpItemType = FtpFileSystemObjectType.File;
                            break;
                        case ItemType.Directory:
                        case ItemType.Drive:
                            ftpItemType = FtpFileSystemObjectType.Directory;
                            break;
                        default:
                            throw new NotSupportedException("Invalid item type:" + type);
                    }
                    if (item.Type != ftpItemType) return null;
                }
                var m = CreateModel(item, path);
                if (type != null) m.Type = type.Value;
                return m;
            }
            catch
            {
                NotifyFtpOperationFinished();
                if (swallowException) return null;
                throw;
            }
        }

        private FileSystemItem CreateModel(IFtpListItem item, string path)
        {
            if (item.Type == FtpFileSystemObjectType.Directory && !path.EndsWith("/")) path += "/";
            return new FileSystemItem
                       {
                           Name = item.Name,
                           Type = item.Type == FtpFileSystemObjectType.File ? ItemType.File : ItemType.Directory,
                           Date = item.Modified,
                           Path = path,
                           FullPath = string.Format("{0}:/{1}", _connection.Name, path),
                           Size = item.Type == FtpFileSystemObjectType.Directory ? null : (long?)item.Size
                       };
        }

        public DateTime GetFileModificationTime(string path)
        {
            return FtpClient.GetModifiedTime(path);
        }

        public bool DriveIsReady(string drive)
        {
            try
            {
                FtpClient.SetWorkingDirectory(drive);
                return true;
            }
            catch (FtpException)
            {
                if (!FtpClient.IsConnected) throw;
                return false;
            }
        }

        public FileExistenceInfo FileExists(string path)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                var filename = LocateDirectory(path);
                return FtpClient.GetFileSize(filename);
            }
            catch {}
            NotifyFtpOperationFinished();
            return false;
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
            var result = ms.ToArray();
            ms.Close();
            if (saveToTempFile)
            {
                TempFilePath = Path.Combine(App.DataDirectory, Guid.NewGuid() + ".tmp");
                File.WriteAllBytes(TempFilePath, result);
            }
            return result;
        }

        internal void DownloadFile(string remotePath, Stream stream, long remoteStartPosition = 0, long fileSize = -1)
        {
            var downloadStarted = false;
            long transferred = 0;
            var bufferSize = 0;
            NotifyFtpOperationStarted(true);
            try
            {
                var filename = LocateDirectory(remotePath);
                if (fileSize == -1)
                {
                    fileSize = FtpClient.GetFileSize(filename);
                    if (fileSize == -1) throw new FtpException(string.Format(Resx.ItemNotExistsOnPath ,remotePath));
                }
                _isIdle = false;
                _isAborted = false;
                if (remoteStartPosition != 0) NotifyFtpOperationResumeStart((int) (remoteStartPosition*100/fileSize), remoteStartPosition);

                using (var ftpStream = FtpClient.OpenRead(filename, remoteStartPosition))
                {
                    downloadStarted = true;
                    var buffer = new byte[0x8000];
                    while (!_isAborted && (bufferSize = ftpStream.Read(buffer, 0, 0x8000)) > 0)
                    {
                        if (transferred + bufferSize > fileSize)
                        {
                            bufferSize = (int) (fileSize - transferred);
                            _isAborted = true;
                        }
                        stream.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyFtpOperationProgressChanged((int) (transferred*100/fileSize), bufferSize, transferred, remoteStartPosition);
                    }
                }
                stream.Flush();
                NotifyFtpOperationFinished(stream.Length);
            }
            catch (Exception ex)
            {
                if (downloadStarted)
                    NotifyFtpOperationProgressChanged((int)(transferred * 100 / fileSize), bufferSize, transferred, remoteStartPosition, true);
                NotifyFtpOperationFinished();
                if (_isAborted && ex.Message == "Broken pipe") return;
                throw;
            }
        }

        internal void UploadFile(string remotePath, string localPath, bool append = false)
        {
            NotifyFtpOperationStarted(true);
            FileStream fs = null;
            var uploadedStarted = false;
            long fileSize = 0;
            long transferred = 0;
            var bufferSize = 0;
            long resumeStartPosition = 0;
            try
            {
                _isIdle = false;
                _isAborted = false;
                fileSize = new FileInfo(localPath).Length;
                fs = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                var filename = LocateDirectory(remotePath);
                if (append)
                {
                    var list = FtpClient.GetListing();
                    transferred = list.First(file => file.Name == filename).Size;
                    resumeStartPosition = transferred;
                    fs.Seek(transferred, SeekOrigin.Begin);
                    NotifyFtpOperationResumeStart((int) (transferred*100/fileSize), transferred);
                }
                using (var ftpStream = append ? FtpClient.OpenAppend(filename) : FtpClient.OpenWrite(filename))
                {
                    uploadedStarted = true;
                    var buffer = new byte[0x8000];
                    while (!_isAborted && (bufferSize = fs.Read(buffer, 0, 0x8000)) > 0)
                    {
                        //if ((int)(transferred * 100 / fileSize) == 50) throw new FtpException("[TESTING] Intentional error at 50%");
                        ftpStream.Write(buffer, 0, bufferSize);
                        transferred += bufferSize;
                        NotifyFtpOperationProgressChanged((int) (transferred*100/fileSize), bufferSize, transferred, resumeStartPosition);
                    }
                }
            }
            catch
            {
                if (uploadedStarted)
                    NotifyFtpOperationProgressChanged((int) (transferred*100/fileSize), bufferSize, transferred, resumeStartPosition, true);
                throw;
            }
            finally
            {
                NotifyFtpOperationFinished();
                if (fs != null) fs.Close();
            }
        }

        public bool FolderExists(string path)
        {
            return FtpClient.DirectoryExists(path);
        }

        public void DeleteFolder(string path)
        {
            FtpClient.DeleteDirectory(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public void DeleteFile(string path)
        {
            FtpClient.DeleteFile(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public void CreateFolder(string path)
        {
            FtpClient.CreateDirectory(ServerType == FtpServerType.MinFTPD ? LocateDirectory(path) : path);
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var oldName = LocateDirectory(path);
            FtpClient.Rename(oldName, newName);
            var r = new Regex(string.Format("{0}{1}?$", Regex.Escape(oldName), Slash), RegexOptions.IgnoreCase);
            path = r.Replace(path, newName);
            var item = GetItemInfo(path);
            if (item == null) throw new ApplicationException(string.Format(Resx.ItemNotExistsOnPath, path));
            return item;
        }

        public void RestoreConnection()
        {
            Connect(_connection);
        }

        private void NotifyFtpOperationStarted(bool binaryTransfer)
        {
            _eventAggregator.GetEvent<FtpOperationStartedEvent>().Publish(new FtpOperationStartedEventArgs(binaryTransfer));
            _notificationTimer.Restart();
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            _isIdle = true;
            _eventAggregator.GetEvent<FtpOperationFinishedEvent>().Publish(new FtpOperationFinishedEventArgs(streamLength));
            _notificationTimer.Stop();
        }

        private void NotifyFtpOperationProgressChanged(int percentage, long transferred, long totalBytesTransferred, long resumeStartPosition, bool force = false)
        {
            _aggregatedTransferredValue += transferred;
            if (!force && _notificationTimer.Elapsed.TotalMilliseconds < 100 && percentage != 100) return;

            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, _aggregatedTransferredValue, totalBytesTransferred, resumeStartPosition));

            _notificationTimer.Restart();
            _aggregatedTransferredValue = 0;
        }

        private void NotifyFtpOperationResumeStart(int percentage, long resumeStartPosition)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, resumeStartPosition, resumeStartPosition, resumeStartPosition));
        }

        public void Abort()
        {
            _isAborted = true;
        }

        #region Inherited TraceListener members

        public override void Write(string message)
        {
            lock (Log)
            {
                if (!message.EndsWith(Environment.NewLine)) message = Log.Pop() + message;
                Log.Push(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            Write(message);
        }

        #endregion
    }
}