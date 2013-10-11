using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Limilabs.FTP.Client;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Exceptions;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    public class FtpWrapper : IFileManager
    {
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;
        private string _address;
        private int _port;
        private string _username;
        private string _password;

        public event FtpOperationStartedEventHandler FtpOperationStarted;
        public event FtpOperationFinishedEventHandler FtpOperationFinished;
        public event FtpOperationProgressChangedEventHandler FtpOperationProgressChanged;

        internal bool Connect(string address, int port, string username, string password)
        {
            try
            {
                _address = address;
                _port = port;
                _username = username;
                _password = password;
                _ftpClient = new Ftp();
                _ftpClient.Connect(address, port);
                _ftpClient.Login(username, password);

                //HACK: FSD FTP states that it supports SIZE command, but it throws a "not implemented" exception
                var mi = _ftpClient.Extensions.GetType().GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_ftpClient.Extensions, new object[] { false });

                _ftpClient.Progress += FtpClientProgressChanged;
            }
            catch (FtpException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
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
                    }).ToList();
                _ftpClient.ChangeFolder(currentFolder);
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
            } 
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            NotifyFtpOperationStarted(false);
            try
            {
                if (path != null) _ftpClient.ChangeFolder(path);
                var currentPath = _ftpClient.GetCurrentFolder();

                var result = _ftpClient.GetList().Select(di => new FileSystemItem
                    {
                        Name = di.Name,
                        Type = di.IsFolder ? ItemType.Directory : ItemType.File,
                        Date = di.ModifyDate,
                        Path = string.Format("{0}/{1}{2}", currentPath, di.Name, di.IsFolder ? "/" : string.Empty),
                        Size = di.Size
                    }).ToList();
                NotifyFtpOperationFinished();
                return result;
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
            }          
        }

        private string LocateDirectory(string path)
        {
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            _ftpClient.ChangeFolder(dir);
            return filename;
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
            }
        }

        public bool DriveIsReady(string drive)
        {
            try
            {
                _ftpClient.ChangeFolder(drive);
                return true;
            }
            catch
            {
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

        public byte[] ReadFileContent(string path)
        {
            return DownloadFile(path);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
            }
        }

        internal void DownloadFile(string remotePath, string localPath, FileMode mode, long remoteStartPosition = 0)
        {
            NotifyFtpOperationStarted(true);
            try
            {
                var fs = new FileStream(localPath, mode);
                _ftpClient.Download(LocateDirectory(remotePath), remoteStartPosition, fs);
                NotifyFtpOperationFinished(fs.Length);
            }
            catch (IOException ex)
            {
                NotifyFtpOperationFinished();
                throw new TransferException(TransferErrorType.WriteAccessError, ex.Message);
            }
            catch (FtpException ex)
            {
                NotifyFtpOperationFinished();
                if (_ftpClient.Connected) throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
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
                throw new TransferException(TransferErrorType.LostConnection, ex.Message);
            }
        }

        public void RestoreConnection()
        {
            _ftpClient.Progress -= FtpClientProgressChanged;
            Connect(_address, _port, _username, _password);
        }

        private void NotifyFtpOperationStarted(bool binaryTransfer)
        {
            var handler = FtpOperationStarted;
            if (handler != null) handler.Invoke(this, new FtpOperationStartedEventArgs(binaryTransfer));
        }

        private void NotifyFtpOperationFinished(long? streamLength = null)
        {
            var handler = FtpOperationFinished;
            if (handler != null) handler.Invoke(this, new FtpOperationFinishedEventArgs(streamLength));
        }

        private void NotifyFtpOperationProgressChanged(int percentage)
        {
            var handler = FtpOperationProgressChanged;
            if (handler != null) handler.Invoke(this, new FtpOperationProgressChangedEventArgs(percentage));
        }
    }
}