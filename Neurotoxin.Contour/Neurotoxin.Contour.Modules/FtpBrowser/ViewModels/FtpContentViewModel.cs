using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Limilabs.FTP.Client;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Neurotoxin.Contour.Core.Extensions;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FtpContentViewModel : PaneViewModelBase
    {
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;

        public FtpContentViewModel(ModuleViewModelBase parent) : base(parent)
        {
        }

        protected override List<FileSystemItem> ChangeDirectory()
        {
            EnsureConnection();
            var content = new List<FileSystemItem>();
            var dotFolder = Stack.Peek();
            if (Stack.Count > 1)
            {
                var dotDotFolder = Stack.ElementAt(1);
                content.Add(new FileSystemItem
                    {
                        Title = "[..]",
                        Type = dotDotFolder.Type,
                        Date = dotDotFolder.Date,
                        Path = dotDotFolder.Path,
                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                    });
            }

            var list = GetList(SelectedPath);

            foreach (var di in list.Where(item => item.IsFolder))
            {
                //var md5 = MD5.Create();
                //var hash = md5.ComputeHash(SelectedPath.ToByteArray());
                //var tmpPath = string.Format("tmp/{0}_{1}", di.Name, hash.ToHex());
                var tmpPath = string.Format("tmp/{0}", di.Name);
                var ftpItem = new FileSystemItem
                {
                    TitleId = di.Name,
                    Type = ItemType.Directory,
                    Date = di.ModifyDate,
                    Path = string.Format("{0}{1}/", SelectedPath, di.Name)
                };

                var cachedItem = LoadCache(tmpPath);
                var hasCache = cachedItem != null;
                if (hasCache)
                {
                    ftpItem.Subtype = cachedItem.Subtype;
                    ftpItem.Title = cachedItem.Title;
                    ftpItem.Thumbnail = cachedItem.Thumbnail;
                }

                switch (dotFolder.Subtype)
                {
                    case ItemSubtype.Content:
                        if (!hasCache && GetProfile(Drive, di.Name, ftpItem)) SaveCache(ftpItem, tmpPath);
                        break;
                    case ItemSubtype.GamesFolder:
                        ftpItem.Subtype = ItemSubtype.Game;
                        if (!hasCache && (GetGameData(Drive, di.Name, ftpItem) || GetGameDataFromJqe360(di.Name, ftpItem)))
                            SaveCache(ftpItem, tmpPath);
                        break;
                    case ItemSubtype.Profile:
                        ftpItem.Subtype = ItemSubtype.GameSaves;
                        if (!hasCache && (GetGameData(Drive, di.Name, ftpItem) || GetGameDataFromJqe360(di.Name, ftpItem)))
                            SaveCache(ftpItem, tmpPath);
                        break;
                }

                switch (di.Name)
                {
                    case "Content":
                        ftpItem.Title = "Content";
                        ftpItem.TitleId = null;
                        ftpItem.Subtype = ItemSubtype.Content;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case "0000000000000000":
                        ftpItem.Title = "Games";
                        ftpItem.TitleId = null;
                        ftpItem.Subtype = ItemSubtype.GamesFolder;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case "00000002":
                        ftpItem.Title = "Downloadable Contents";
                        ftpItem.Subtype = ItemSubtype.DownloadableContents;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case "00007000":
                        ftpItem.Title = "GOD Contents";
                        ftpItem.Subtype = ItemSubtype.GameOnDemand;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case "000B0000":
                        ftpItem.Title = "Title Updates";
                        ftpItem.Subtype = ItemSubtype.TitleUpdates;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case "FFFE07D1":
                        ftpItem.Title = "Profile Data";
                        ftpItem.Subtype = ItemSubtype.Undefined;
                        ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    default:
                        if (ftpItem.Thumbnail == null)
                            ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                }

                content.Add(ftpItem);
            }

            foreach (var fi in list.Where(item => item.IsFile))
            {
                var tmpPath = string.Format("tmp/{0}", fi.Name);
                var ftpItem = new FileSystemItem
                    {
                        Title = fi.Name,
                        Type = ItemType.File,
                        Date = fi.ModifyDate,
                        Path = string.Format("{0}{1}", SelectedPath, fi.Name),
                        Size = fi.Size,
                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/file.png")
                    };
                var cachedItem = LoadCache(tmpPath);
                var hasCache = cachedItem != null;
                if (hasCache)
                {
                    ftpItem.Subtype = cachedItem.Subtype;
                    ftpItem.Title = cachedItem.Title;
                    ftpItem.Thumbnail = cachedItem.Thumbnail;
                }
                else
                {
                    switch (dotFolder.Subtype)
                    {
                        case ItemSubtype.TitleUpdates:
                            ftpItem.Subtype = ItemSubtype.TitleUpdate;
                            break;
                        case ItemSubtype.DownloadableContents:
                            ftpItem.Subtype = ItemSubtype.DownloadableContent;
                            break;
                    }

                    if (Path.GetExtension(fi.Name) == string.Empty)
                    {
                        var header = DownloadHeader(ftpItem.Path);
                        var svod = ModelFactory.GetModel<SvodPackage>(header);
                        if (svod.IsValid)
                        {
                            ftpItem.Title = svod.DisplayName;
                            ftpItem.Thumbnail = svod.ThumbnailImage;
                            SaveCache(ftpItem, tmpPath);
                        }
                    }
                }
                content.Add(ftpItem);
            }
            return content;
        }

        protected override long CalculateSize(string path)
        {
            EnsureConnection();
            var list = GetList(path);
            return list.Where(item => item.IsFile).Sum(fi => fi.Size.HasValue ? fi.Size.Value : 0)
                 + list.Where(item => item.IsFolder).Sum(di => CalculateSize(string.Format("{0}{1}/", path, di.Name)));
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    Parent.IsInProgress = true;
                    WorkerThread.Run(Connect, ConnectCallback);
                    break;
            }
        }

        private bool Connect()
        {
            EnsureConnection();
            return true;
        }

        private void ConnectCallback(bool success)
        {
            Parent.IsInProgress = false;
            Parent.StatusBarText = "Connected.";
            Drive = "Hdd1";
            Stack = new Stack<FileSystemItemViewModel>();
            var root = new FileSystemItemViewModel
                {
                    Path = string.Format("/{0}/", Drive),
                    Title = Drive,
                    Type = ItemType.Directory
                };
            Stack.Push(root);
            ChangeDirectoryCommand.Execute(root.Path);
        }

        private bool GetProfile(string drive, string profileId, FileSystemItem fileSystemItem)
        {
            var profilePath = string.Format("/{1}/Content/{0}/FFFE07D1/00010000/{0}", profileId, drive);
            if (!_ftpClient.FileExists(profilePath)) return false;

            var fileContent = DownloadFile(profilePath);
            var stfs = ModelFactory.GetModel<StfsPackage>(fileContent);
            stfs.ExtractAccount();
            fileSystemItem.Title = stfs.Account.GamerTag;
            fileSystemItem.Thumbnail = stfs.ThumbnailImage;
            fileSystemItem.Subtype = ItemSubtype.Profile;
            return true;
        }

        private bool GetGameData(string drive, string titleId, FileSystemItem fileSystemItem)
        {
            var gamePath = string.Format("/{1}/Content/0000000000000000/{0}/00007000/", titleId, drive);
            var infoFileFound = false;
            if (_ftpClient.FolderExists(gamePath))
            {
                var file = GetList(gamePath).FirstOrDefault(item => item.IsFile);
                if (file != null)
                {
                    var fileContent = DownloadFile(string.Format("{0}{1}", gamePath, file.Name));
                    var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                    fileSystemItem.Title = svod.TitleName;
                    fileSystemItem.Thumbnail = svod.ThumbnailImage;
                    fileSystemItem.Subtype = ItemSubtype.Game;
                    infoFileFound = true;
                }
            }
            return infoFileFound;
        }

        private bool GetGameDataFromJqe360(string titleId, FileSystemItem fileSystemItem)
        {
            var request = WebRequest.Create(string.Format("http://covers.jqe360.com/main.php?search={0}", titleId));
            string title = null;
            var result = false;
            try
            {
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    var responseReader = new StreamReader(stream);
                    var htmlText = responseReader.ReadToEnd();
                    var regex = new Regex(string.Format("Title: .*?>(.*?)<.*?TitleID: {0}", titleId));
                    title = regex.Match(htmlText).Groups[1].Value;
                    result = true;
                }
            }
            catch {}
            fileSystemItem.Title = title;
            fileSystemItem.Subtype = ItemSubtype.Game;
            fileSystemItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            return result;
        }

        public void DownloadAll(string toPath)
        {
            //UNDONE: support for append
            foreach (var item in Content.Where(item => item.IsSelected))
            {
                var fs = new FileStream(Path.Combine(toPath, item.Title), FileMode.OpenOrCreate);
                DownloadFile(item.Path, fs);
                fs.Flush(true);
                fs.Close();
            }
        }

        public void UploadAll(IEnumerable<string> localFiles)
        {
            //UNDONE: support for append
            foreach (var localPath in localFiles)
            {
                var fileName = Path.GetFileName(localPath);
                var remotePath = string.Format("{0}{1}", SelectedPath, fileName);
                if (File.Exists(localPath))
                {
                    UploadFile(remotePath, localPath);
                }
                if (Directory.Exists(localPath))
                {
                    CreateFolder(remotePath);
                    UploadAll(Directory.GetDirectories(localPath).Concat(Directory.GetFiles(localPath)));
                }
            }
        }

        public override void DeleteAll()
        {
            foreach (var item in Content.Where(item => item.IsSelected))
            {
                if (item.Type == ItemType.Directory)
                {
                    _ftpClient.DeleteFolderRecursively(item.Path);
                } 
                else
                {
                    _ftpClient.DeleteFile(item.Path);
                }
            }
        }

        public override void CreateFolder(string name)
        {
            _ftpClient.CreateFolder(string.Format("{0}{1}", SelectedPath, name));
        }

        private void EnsureConnection()
        {
            if (_ftpClient != null && _ftpClient.Connected) return;
            _ftpClient = new Ftp();
            _ftpClient.Connect("127.0.0.1");
            _ftpClient.Login("xbox", "hardcore21*");
            _ftpClient.Progress += FtpClientProgressChanged;
        }

        private T AsyncOperation<T>(Func<T> func, int queueLength)
        {
            UIThread.Run(() =>
            {
                Parent.IsInProgress = true;
                Parent.LoadingQueueLength = queueLength;
                Parent.LoadingProgress = 0;
            });
            var result = func.Invoke();
            UIThread.Run(() => Parent.IsInProgress = false);
            return result;
        }

        #region FTP wrap up

        private List<FtpItem> GetList(string path)
        {
            return AsyncOperation(() => _ftpClient.GetList(path), 1);
        }

        private byte[] DownloadFile(string path)
        {
            var file = AsyncOperation(() => _ftpClient.Download(path), 100);
            UIThread.Run(() => Parent.StatusBarText = string.Format("Downloaded {0} bytes.", file.Length));
            return file;
        }

        private void DownloadFile(string path, Stream stream)
        {
            AsyncOperation(() => { _ftpClient.Download(path, stream); return true; }, 100);
            UIThread.Run(() => Parent.StatusBarText = string.Format("Downloaded {0} bytes.", stream.Length));
        }

        private byte[] DownloadHeader(string path)
        {
            var stream = AsyncOperation(() =>
            {
                _downloadHeaderOnly = true;
                var ms = new MemoryStream();
                try
                {
                    _ftpClient.Download(path, ms);
                }
                catch
                {
                    //NOTE: this is intentional, unfortunately the ftp client will throw an exception after the Abort()
                }
                _downloadHeaderOnly = false;
                return ms;
            }, 1);
            UIThread.Run(() => Parent.StatusBarText = string.Format("Downloaded {0} bytes.", stream.Length));
            return stream.ToArray();
        }

        private void UploadFile(string remotePath, string localPath)
        {
            AsyncOperation(() => { _ftpClient.Upload(remotePath, localPath); return true; }, 100);
            UIThread.Run(() => Parent.StatusBarText = string.Format("Uploaded {0}.", localPath));
        }

        private void FtpClientProgressChanged(object sender, ProgressEventArgs e)
        {
            UIThread.Run(() => { Parent.LoadingProgress = (int)e.Percentage; });
            if (_downloadHeaderOnly && e.TotalBytesTransferred > 0x971A) // v1 header size
                _ftpClient.Abort();
        }

        #endregion
    }
}