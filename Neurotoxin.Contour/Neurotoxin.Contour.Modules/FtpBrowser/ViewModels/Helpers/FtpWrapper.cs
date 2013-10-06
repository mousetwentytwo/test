using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Limilabs.FTP.Client;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    public class FtpWrapper : IFileManager
    {
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;

        public event FtpOperationStartedEvent FtpOperationStarted;
        public event FtpOperationFinishedEvent FtpOperationFinished;
        public event FtpOperationProgressChangedEvent FtpOperationProgressChanged;

        internal bool Connect()
        {
            _ftpClient = new Ftp();
            _ftpClient.Connect("127.0.0.1");
            _ftpClient.Login("xbox", "hardcore21*");
            //_ftpClient.Connect("192.168.1.110");
            //_ftpClient.Login("xbox", "xbox");

            //HACK: FSD FTP states that it supports SIZE command, but it throws a "not implemented" exception
            var mi = _ftpClient.Extensions.GetType().GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(_ftpClient.Extensions, new object[] { false });

            _ftpClient.Progress += FtpClientProgressChanged;
            return true;
        }

        public List<FileSystemItem> GetDrives()
        {
            NotifyFtpOperationStarted(false);
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

        public List<FileSystemItem> GetList(string path = null)
        {
            NotifyFtpOperationStarted(false);
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

        private string LocateDirectory(string path)
        {
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            _ftpClient.ChangeFolder(dir);
            return filename;
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
            var result = _ftpClient.Download(LocateDirectory(path));
            NotifyFtpOperationFinished(result.Length);
            return result;
        }

        internal void DownloadFile(string remotePath, string localPath)
        {
            var fs = new FileStream(localPath, FileMode.OpenOrCreate);
            DownloadFile(remotePath, fs);
            fs.Flush();
            fs.Close();
        }

        internal void DownloadFile(string path, Stream stream)
        {
            NotifyFtpOperationStarted(true);
            _ftpClient.Download(LocateDirectory(path), stream);
            NotifyFtpOperationFinished(stream.Length);
        }

        internal byte[] DownloadHeader(string path)
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
            Connect();
            NotifyFtpOperationFinished(ms.Length);
            return ms.ToArray();
        }

        internal void UploadFile(string remotePath, string localPath)
        {
            NotifyFtpOperationStarted(true);
            _ftpClient.Upload(remotePath, localPath);
            NotifyFtpOperationFinished();
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
            _ftpClient.DeleteFolderRecursively(path);
        }

        public void DeleteFile(string path)
        {
            _ftpClient.DeleteFile(path);
        }

        public void CreateFolder(string path)
        {
            _ftpClient.CreateFolder(path);
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