using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using Limilabs.FTP.Client;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FtpContentViewModel : PaneViewModelBase
    {
        private Ftp _ftpClient;
        private bool _downloadHeaderOnly;

        #region RefreshTitleCommand

        public DelegateCommand<FileSystemItemViewModel> RefreshTitleCommand { get; private set; }

        private void ExecuteRefreshTitleCommand(FileSystemItemViewModel cmdParam)
        {
            WorkerThread.Run(RefreshTitle, RefreshTitleCallback);
        }

        private bool CanExecuteRefreshTitleCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        private FileSystemItemViewModel RefreshTitle()
        {
            var result = CurrentRow;
            RecognizeTitle(CurrentRow.Model);
            return result;
        }

        private void RefreshTitleCallback(FileSystemItemViewModel item)
        {
            item.NotifyModelChanges();
        }

        #endregion

        #region CopyTitleIdToClipboardCommand

        public DelegateCommand<FileSystemItemViewModel> CopyTitleIdToClipboardCommand { get; private set; }

        private void ExecuteCopyTitleIdToClipboardCommand(FileSystemItemViewModel cmdParam)
        {
            Clipboard.SetData(DataFormats.Text, cmdParam.TitleId);
        }

        private bool CanExecuteCopyTitleIdToClipboardCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand<FileSystemItemViewModel> SearchGoogleCommand { get; private set; }

        private void ExecuteSearchGoogleCommand(FileSystemItemViewModel cmdParam)
        {
            System.Diagnostics.Process.Start(string.Format("http://www.google.com/#q={0}", cmdParam.TitleId));
        }

        private bool CanExecuteSearchGoogleCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        #endregion

        public FtpContentViewModel(ModuleViewModelBase parent) : base(parent)
        {
            RefreshTitleCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            RefreshTitleCommand.RaiseCanExecuteChanged();
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
        }

        protected override List<FileSystemItem> ChangeDirectory()
        {
            EnsureConnection();
            var content = new List<FileSystemItem>();
            if (Stack.Count > 1)
            {
                var parentFolder = Stack.ElementAt(1);
                content.Add(new FileSystemItem
                {
                    Title = "[..]",
                    Type = parentFolder.Type,
                    Date = parentFolder.Date,
                    Path = parentFolder.Path,
                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                });
            }

            var selectedPath = CurrentFolder.Path;
            var list = GetList(selectedPath);

            foreach (var di in list)
            {
                var tmpPath = string.Format("tmp/{0}", di.Name);

                var ftpItem = di.IsFolder
                                  ? new FileSystemItem
                                        {
                                            TitleId = di.Name,
                                            Type = ItemType.Directory,
                                            Date = di.ModifyDate,
                                            Path = string.Format("{0}{1}/", selectedPath, di.Name),
                                            Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png")
                                        }
                                  : new FileSystemItem
                                        {
                                            Title = di.Name,
                                            Type = ItemType.File,
                                            Date = di.ModifyDate,
                                            Path = string.Format("{0}{1}", selectedPath, di.Name),
                                            Size = di.Size,
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
                    RecognizeTitle(ftpItem);
                }

                switch (di.Name)
                {
                    case "Content":
                        ftpItem.Title = "[Content]";
                        ftpItem.TitleId = null;
                        ftpItem.Subtype = ItemSubtype.Content;
                        break;
                    case "0000000000000000":
                        ftpItem.Title = "[Games]";
                        ftpItem.TitleId = null;
                        ftpItem.Subtype = ItemSubtype.GamesFolder;
                        break;
                    case "00000002":
                        ftpItem.Title = "[Downloadable Contents]";
                        ftpItem.Subtype = ItemSubtype.DownloadableContents;
                        break;
                    case "00007000":
                        ftpItem.Title = "[GOD Contents]";
                        ftpItem.Subtype = ItemSubtype.GameOnDemand;
                        break;
                    case "00009000":
                        ftpItem.Title = "[Avatar Items]";
                        ftpItem.Subtype = ItemSubtype.AvatarItems;
                        break;
                    case "000D0000":
                        ftpItem.Title = "[XBLA Contents]";
                        ftpItem.Subtype = ItemSubtype.XboxLiveArcadeGame;
                        break;
                    case "000B0000":
                        ftpItem.Title = "[Title Updates]";
                        ftpItem.Subtype = ItemSubtype.TitleUpdates;
                        break;
                    case "584E07D2":
                        ftpItem.Title = "XNA Indie Player";
                        break;
                    default:
                        if (di.Name.StartsWith("FFFE"))
                        {
                            ftpItem.Title = "[System Data]";
                            ftpItem.Subtype = ItemSubtype.Undefined;
                            ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        }
                        break;
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
            Stack = new Stack<FileSystemItem>();
            var root = new FileSystemItem
                {
                    Path = string.Format("/{0}/", Drive),
                    Title = Drive,
                    Type = ItemType.Directory
                };
            Stack.Push(root);
            ChangeDirectoryCommand.Execute(root.Path);
        }

        private void RecognizeTitle(FileSystemItem ftpItem)
        {
            var currentFolder = Stack.Peek();
            var tmpPath = string.Format("tmp/{0}", ftpItem.TitleId);

            switch (currentFolder.Subtype)
            {
                case ItemSubtype.Content:
                    var profilePath = string.Format("/{1}/Content/{0}/FFFE07D1/00010000/{0}", ftpItem.TitleId, Drive);
                    if (GetProfileData(ftpItem, profilePath)) SaveCache(ftpItem, tmpPath);
                    break;
                case ItemSubtype.GamesFolder:
                    ftpItem.Subtype = ItemSubtype.Game;
                    if (GetGameData(ftpItem) || GetGameDataFromJqe360(ftpItem)) SaveCache(ftpItem, tmpPath);
                    break;
                case ItemSubtype.Profile:
                    ftpItem.Subtype = ItemSubtype.GameSaves;
                    if (GetGameData(ftpItem) || GetGameDataFromJqe360(ftpItem)) SaveCache(ftpItem, tmpPath);
                    break;
                case ItemSubtype.TitleUpdates:
                    ftpItem.Subtype = ItemSubtype.TitleUpdate;
                    break;
                case ItemSubtype.DownloadableContents:
                    ftpItem.Subtype = ItemSubtype.DownloadableContent;
                    break;
            }

            if (ftpItem.Type == ItemType.File && Path.GetExtension(ftpItem.Title) == string.Empty)
            {
                tmpPath = string.Format("tmp/{0}", ftpItem.Title);
                switch (ftpItem.Subtype)
                {
                    case ItemSubtype.Profile:
                        GetProfileData(ftpItem, ftpItem.Path);
                        break;
                    default:
                        var header = DownloadHeader(ftpItem.Path);
                        var svod = ModelFactory.GetModel<SvodPackage>(header);
                        if (svod.IsValid)
                        {
                            ftpItem.Title = svod.DisplayName;
                            ftpItem.Thumbnail = svod.ThumbnailImage;
                            SaveCache(ftpItem, tmpPath);
                        }
                        break;
                }
            }
        }

        private bool GetProfileData(FileSystemItem ftpItem, string profilePath = null)
        {
            if (profilePath == null) profilePath = ftpItem.Path;
            if (!FileExists(profilePath)) return false;

            var fileContent = DownloadFile(profilePath);
            var stfs = ModelFactory.GetModel<StfsPackage>(fileContent);
            stfs.ExtractAccount();
            ftpItem.Title = stfs.Account.GamerTag;
            ftpItem.Thumbnail = stfs.ThumbnailImage;
            ftpItem.Subtype = ItemSubtype.Profile;
            return true;
        }

        private bool GetGameData(FileSystemItem ftpItem)
        {
            var infoFileFound = false;

            var systemdir = ftpItem.TitleId.StartsWith("5841") ? "000D0000" : "00007000";

            var gamePath = string.Format("/{1}/Content/0000000000000000/{0}/{2}/", ftpItem.TitleId, Drive, systemdir);
            if (_ftpClient.FolderExists(gamePath))
            {
                var file = GetList(gamePath).FirstOrDefault(item => item.IsFile);
                if (file != null)
                {
                    var fileContent = DownloadHeader(string.Format("{0}{1}", gamePath, file.Name));
                    var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                    if (svod.IsValid)
                    {
                        ftpItem.Title = svod.TitleName;
                        ftpItem.Thumbnail = svod.ThumbnailImage;
                        ftpItem.Subtype = ItemSubtype.Game;
                        infoFileFound = true;
                    }
                }
            }
            return infoFileFound;
        }

        private bool GetGameDataFromJqe360(FileSystemItem ftpItem)
        {
            var request = WebRequest.Create(string.Format("http://covers.jqe360.com/main.php?search={0}", ftpItem.TitleId));
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
                    var regex = new Regex(string.Format("Title: .*?>(.*?)<.*?TitleID: {0}", ftpItem.TitleId), RegexOptions.IgnoreCase);
                    title = regex.Match(htmlText).Groups[1].Value;
                    result = true;
                }
            }
            catch {}
            ftpItem.Title = title;
            ftpItem.Subtype = ItemSubtype.Game;
            ftpItem.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            return result;
        }

        public void DownloadAll(string toPath)
        {
            EnsureConnection();
            //UNDONE: support for append
            foreach (var item in Items.Where(item => item.IsSelected))
            {
                var fs = new FileStream(Path.Combine(toPath, item.Title), FileMode.OpenOrCreate);
                DownloadFile(item.Path, fs);
                fs.Flush(true);
                fs.Close();
            }
        }

        public void UploadAll(IEnumerable<string> localFiles)
        {
            EnsureConnection();
            //UNDONE: support for append
            foreach (var localPath in localFiles)
            {
                var fileName = Path.GetFileName(localPath);
                var remotePath = string.Format("{0}{1}", CurrentRow.Path, fileName);
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
            EnsureConnection();
            foreach (var item in Items.Where(item => item.IsSelected))
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
            EnsureConnection();
            _ftpClient.CreateFolder(string.Format("{0}{1}", CurrentRow.Path, name));
        }

        private void EnsureConnection()
        {
            if (_ftpClient != null)
            {
                try
                {
                    _ftpClient.SendCommand("PASV");
                    return;
                } 
                catch {}
            }
            _ftpClient = new Ftp();
            //_ftpClient.Connect("127.0.0.1");
            //_ftpClient.Login("xbox", "hardcore21*");
            _ftpClient.Connect("192.168.1.110");
            _ftpClient.Login("xbox", "xbox");

            //HACK: FSD FTP states that it supports SIZE command, but it throws a "not implemented" exception
            var mi = _ftpClient.Extensions.GetType().GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(_ftpClient.Extensions, new object[] { false });

            _ftpClient.Progress += FtpClientProgressChanged;
        }

        private T AsyncOperation<T>(Func<T> func, int queueLength = 1)
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
            return AsyncOperation(() =>
                                      {
                                          _ftpClient.ChangeFolder(path);
                                          return _ftpClient.GetList();
                                      });
        }

        private string LocateDirectory(string path)
        {
            var dir = path.Substring(0, path.LastIndexOf('/') + 1);
            var filename = path.Replace(dir, String.Empty);
            _ftpClient.ChangeFolder(dir);
            return filename;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <remarks>Unfortunately the FTP server in FSD doesn't support the FileExists command</remarks>
        /// <returns></returns>
        private bool FileExists(string path)
        {
            return AsyncOperation(() =>
                                      {
                                          try
                                          {
                                              var filename = LocateDirectory(path);
                                              var list = _ftpClient.GetList();
                                              return list.Any(file => file.Name == filename);
                                          } 
                                          catch
                                          {
                                              return false;
                                          }
                                      });
        }

        private byte[] DownloadFile(string path)
        {
            var file = AsyncOperation(() => _ftpClient.Download(LocateDirectory(path)), 100);
            UIThread.Run(() => Parent.StatusBarText = string.Format("Downloaded {0} bytes.", file.Length));
            return file;
        }

        private void DownloadFile(string path, Stream stream)
        {
            AsyncOperation(() => { _ftpClient.Download(LocateDirectory(path), stream); return true; }, 100);
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
                    _ftpClient.Download(LocateDirectory(path), ms);
                }
                catch (Exception ex)
                {
                    //NOTE: this is intentional, unfortunately the ftp client will throw an exception after the Abort()
                }
                _downloadHeaderOnly = false;
                ms.Flush();
                _ftpClient = null;
                EnsureConnection();
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