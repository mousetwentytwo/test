using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Extensions;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Fizzler.Systems.HtmlAgilityPack;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FtpContentViewModel : FileListPaneViewModelBase<FtpContent>
    {
        private readonly HashSet<int> _doContentScanOn = new HashSet<int>();
        private readonly Dictionary<string, string> _driveLabelCache = new Dictionary<string, string>();
        private string _httpSessionId;

        public FtpConnectionItemViewModel Connection { get; private set; }
        public Stack<string> Log { get { return FileManager.Log; } }
        public Dictionary<int, FsdScanPath> ScanFolders { get; private set; }

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
            get { return Resx.Download; }
        }

        protected override string ImportActionDescription
        {
            get { return Resx.Upload; }
        }

        private string ConnectionLostMessage
        {
            get
            {
                return string.Format(Resx.ConnectionLostMessage, Connection.Name);
            }
        }

        private bool IsContentScanTriggerAvailable
        {
            get
            {
                return UserSettings.FsdContentScanTrigger != FsdContentScanTrigger.Disabled &&
                       Connection != null && !Connection.IsHttpAccessDisabled && FileManager.IsFSD;
            }
        }

        public override bool IsVerificationEnabled
        {
            get { return UserSettings.VerifyFileHashAfterFtpUpload && FileManager.IsFSD; }
        }

        #region Command overrides

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

        #region CheckFreestyleDatabaseCommand

        public DelegateCommand CheckFreestyleDatabaseCommand { get; private set; }

        public bool CanExecuteCheckFreestyleDatabaseCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteCheckFreestyleDatabaseCommand()
        {
            eventAggregator.GetEvent<FreestyleDatabaseCheckEvent>().Publish(new FreestyleDatabaseCheckEventArgs(this));
        }

        #endregion

        #region LaunchGameCommand

        public DelegateCommand LaunchGameCommand { get; private set; }

        public bool CanExecuteLaunchGameCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteLaunchGameCommand()
        {
            try
            {
                var scanFolder = GetCorrespondingScanFolder(CurrentRow.Path);
                if (scanFolder != null)
                {
                    var prefix = CurrentRow.Path.Replace("/", "\\").SubstringAfter(scanFolder.Drive);
                    var html = new HtmlDocument();
                    html.LoadHtml(HttpGetString("gettable.html?name=ContentItems"));
                    foreach (var row in html.DocumentNode.QuerySelectorAll("table.GameContentHeader > tr").Skip(2))
                    {
                        var cells = row.SelectNodes("td");
                        if (Int32.Parse(cells[1].InnerText.Trim()) == scanFolder.PathId && cells[6].InnerText.Trim().StartsWith(prefix))
                        {
                            var contentId = Int32.Parse(cells[0].InnerText.Trim());
                            HttpPost("launch", string.Format("sessionid={0}&contentid={1:X2}&Action=launch", _httpSessionId, contentId));
                            ExecuteCloseCommand();
                            return;
                        }
                    }
                    //TODO
                } 
                else
                {
                    //TODO
                }
            }
            catch
            {
                //TODO
            }
        }

        #endregion

        #region LaunchXexCommand

        public DelegateCommand LaunchXexCommand { get; private set; }

        public bool CanExecuteLaunchXexCommand()
        {
            return FileManager.IsFSD;
        }

        public void ExecuteLaunchXexCommand()
        {
            FileManager.Execute(CurrentRow.Path);
            ExecuteCloseCommand();
        }

        #endregion


        public FtpContentViewModel()
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            CheckFreestyleDatabaseCommand = new DelegateCommand(ExecuteCheckFreestyleDatabaseCommand, CanExecuteCheckFreestyleDatabaseCommand);
            LaunchGameCommand = new DelegateCommand(ExecuteLaunchGameCommand, CanExecuteLaunchGameCommand);
            LaunchXexCommand = new DelegateCommand(ExecuteLaunchXexCommand, CanExecuteLaunchXexCommand);
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
                                    if (error != null) error.Invoke(this, new SomethingWentWrongException(Resx.IndetermineFtpConnectionError, ex));
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
            _doContentScanOn.Clear();
            Initialize();
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

            if (!IsContentScanTriggerAvailable) return;
            var username = Connection.HttpUsername;
            var password = Connection.Password;
            if (username == null)
            {
                switch (FileManager.ServerType)
                {
                    case FtpServerType.F3:
                        username = password = "f3http";
                        break;
                    case FtpServerType.FSD:
                        username = password = "fsdhttp";
                        break;
                    default:
                        throw new NotSupportedException("Invalid server type: " + FileManager.ServerType);
                }
            }

            switch (GetScanFolders(username, password))
            {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.Unauthorized:
                    bool result;
                    do
                    {
                        var login = LoginDialog.Show(Resx.Login, Resx.LoginToFreestyleDashHttpServer, username, password);
                        if (login == null)
                        {
                            var answer = (DisableOption)InputDialog.ShowList(Resx.DisableFsdContentScanTriggerTitle, Resx.DisableFsdContentScanTriggerMessage, DisableOption.None, GetDisableOptionList());
                            switch (answer)
                            {
                                case DisableOption.All:
                                    UserSettings.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
                                    break;
                                case DisableOption.Single:
                                    Connection.IsHttpAccessDisabled = true;
                                    break;
                            }
                            result = true;
                        }
                        else
                        {
                            username = login.Username;
                            password = login.Password;
                            var status = GetScanFolders(username, password);
                            if (status != HttpStatusCode.OK && status != HttpStatusCode.Unauthorized)
                            {
                                //TODO: handle different result then the previous one
                                result = true;
                            }
                            else
                            {
                                result = status != HttpStatusCode.Unauthorized;
                                if (result && login.RememberPassword)
                                {
                                    Connection.HttpUsername = username;
                                    Connection.HttpPassword = password;
                                    eventAggregator.GetEvent<ConnectionDetailsChangedEvent>().Publish(new ConnectionDetailsChangedEventArgs(Connection));
                                }
                            }
                        }
                    } while (!result);
                    break;
                case HttpStatusCode.RequestTimeout:
                    {
                        var answer = (DisableOption)InputDialog.ShowList(Resx.DisableFsdContentScanTriggerTitle, Resx.DisableFsdContentScanTriggerMessage, DisableOption.None, GetDisableOptionList());
                        switch (answer)
                        {
                            case DisableOption.All:
                                UserSettings.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
                                break;
                            case DisableOption.Single:
                                Connection.IsHttpAccessDisabled = true;
                                break;
                        }
                    }
                    break;
            }
        }

        private List<InputDialogOptionViewModel> GetDisableOptionList()
        {
            return Enum.GetValues(typeof (DisableOption))
                       .Cast<DisableOption>()
                       .Select(o => new InputDialogOptionViewModel
                       {
                           Value = o,
                           DisplayName = Resx.ResourceManager.EnumToTranslation(o)
                       })
                       .ToList();
        }

        public void RestoreConnection()
        {
            FileManager.RestoreConnection();
        }

        public override void Abort()
        {
            FileManager.Abort();
        }

        public override void FinishTransferAsSource()
        {
            IsKeepAliveEnabled = false;
        }

        public override void FinishTransferAsTarget()
        {
            IsKeepAliveEnabled = false;

            if (!IsContentScanTriggerAvailable) return;

            var scanFolder = GetCorrespondingScanFolder(CurrentFolder.Path);
            if (scanFolder == null) return;

            switch (UserSettings.FsdContentScanTrigger)
            {
                case FsdContentScanTrigger.AfterUpload:
                    TriggerContentScan(scanFolder.PathId);
                    break;
                case FsdContentScanTrigger.AfterConnectionClose:
                    _doContentScanOn.Add(scanFolder.PathId);
                    break;
            }
        }

        private HttpStatusCode GetScanFolders(string username, string password)
        {
            try
            {
                using (var client = new WebClient())
                {
                    byte[] response;
                    var url = string.Format("http://{0}/paths.html", Connection.Address);
                    if (username == string.Empty)
                    {
                        response = client.DownloadData(url);
                    }
                    else
                    {
                        byte[] body;
                        using (var ms = new MemoryStream())
                        {
                            var sw = new StreamWriter(ms);
                            sw.Write("j_username={0}&j_password={1}&Action=Login", username, password);
                            sw.Flush();
                            body = ms.ToArray();
                        }
                        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        response = client.UploadData(url, body);
                    }
                    var responseString = Encoding.UTF8.GetString(response);
                    if (responseString.Contains("j_password")) return HttpStatusCode.Unauthorized;

                    var r = new Regex(@"<tr.*?pathid:'(?<PathId>\d+)'.*?depth"">(?<ScanDepth>\d+).*?path"">(?<Path>.*?)</td>", RegexOptions.Singleline);
                    ScanFolders = new Dictionary<int, FsdScanPath>();
                    foreach (Match m in r.Matches(responseString))
                    {
                        try
                        {
                            var path = "/" + m.Groups["Path"].Value.Replace(":\\", "\\").Replace("\\", "/");
                            if (ScanFolders.Any(kvp => kvp.Value.Path == path)) continue;
                            var pathid = Int32.Parse(m.Groups["PathId"].Value);
                            var depth = Int32.Parse(m.Groups["ScanDepth"].Value);
                            var drive = m.Groups["Path"].Value.SubstringBefore(":");
                            ScanFolders.Add(pathid, new FsdScanPath
                            {
                                PathId = pathid,
                                Path = path,
                                ScanDepth = depth,
                                Drive = drive
                            });
                        }
                        catch
                        {
                        }
                    }

                    const string sessionCookie = "session=";
                    var setCookie = client.ResponseHeaders[HttpResponseHeader.SetCookie].Split('&');
                    var session = setCookie.FirstOrDefault(c => c.StartsWith(sessionCookie));
                    if (session != null) _httpSessionId = session.Substring(sessionCookie.Length);

                    return HttpStatusCode.OK;
                }
            }
            catch
            {
                return HttpStatusCode.RequestTimeout;
            }
        }

        private FsdScanPath GetCorrespondingScanFolder(string path)
        {
            foreach (var scanPath in ScanFolders.Where(s => path.StartsWith(s.Value.Path)).Select(s => s.Value))
            {
                var relativePath = path.Replace(scanPath.Path, String.Empty).Trim('/');
                var depth = relativePath.Split('/').Length;
                if (scanPath.ScanDepth >= depth) return scanPath;
            }
            return null;
        }

        private void TriggerContentScan(int pathid)
        {
            try
            {
                HttpPost("paths.html", string.Format("sessionid={0}&pathid={1}&Action=scan", _httpSessionId, pathid));
            }
            catch
            {
                WindowManager.ShowMessage(Resx.FsdContentScanTrigger, Resx.ContentScanFailedErrorMessage);
            }
        }

        public byte[] HttpGet(string target)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Cookie] = "session=" + _httpSessionId;
                var result = client.DownloadData(string.Format("http://{0}/{1}", Connection.Address, target));
                return result;
            }
        }

        public string HttpGetString(string target, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            return encoding.GetString(HttpGet(target));
        }

        private void HttpPost(string target, string formData)
        {
            using (var client = new WebClient())
            {
                byte[] body;
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);
                    sw.Write(formData);
                    sw.Flush();
                    body = ms.ToArray();
                }
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.Headers[HttpRequestHeader.Cookie] = "session=" + _httpSessionId;
                client.UploadDataAsync(new Uri(string.Format("http://{0}/{1}", Connection.Address, target)), body);
            }
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

        protected override IList<FileSystemItem> ChangeDirectoryInner(string selectedPath)
        {
            return FileManager.ServerType == FtpServerType.PlayStation3
                ? FileManager.GetList(selectedPath)
                : base.ChangeDirectoryInner(selectedPath);
        }

        protected override void ChangeDirectoryCallback(IList<FileSystemItem> result)
        {
            base.ChangeDirectoryCallback(result);
            if (FileManager.ServerType != FtpServerType.PlayStation3) return;

            //PS3 Transfer complete response string
            var r = new Regex(string.Format(@"226 Transfer complete \[{0}\] \[ ([0-9]+.*?free) \]", Drive.Path.TrimEnd('/')));
            var m = r.Match(FileManager.Log.ElementAt(1));
            if (m.Success)
            {
                //TODO: localize
                FreeSpace = m.Groups[1].Value;
            }
        }

        public override string GetTargetPath(string path)
        {
            return String.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override bool SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            return FileManager.DownloadFile(item.Path, fs, remoteStartPosition, item.Size ?? 0);
        }

        protected override Exception WrapTransferRelatedExceptions(Exception exception)
        {
            if (!FileManager.IsConnected)
            {
                return new TransferException(TransferErrorType.LostConnection, ConnectionLostMessage, exception);
            }
            if (exception is IOException || exception is FtpException || exception is SocketException)
            {
                return new TransferException(TransferErrorType.NotSpecified, exception.Message, exception);
            }
            return base.WrapTransferRelatedExceptions(exception);
        }

        protected override bool CreateFile(string targetPath, FileSystemItem source)
        {
            return FileManager.UploadFile(targetPath, source.Path);
        }

        protected override bool OverwriteFile(string targetPath, FileSystemItem source)
        {
            return FileManager.UploadFile(targetPath, source.Path);
        }

        protected override bool ResumeFile(string targetPath, FileSystemItem source)
        {
            return FileManager.UploadFile(targetPath, source.Path, true);
        }

        public TransferResult VerifyUpload(string savePath, string itemPath)
        {
            if (!FileManager.IsFSD) return TransferResult.Skipped;

            bool match;
            try
            {
                match = FileManager.VerifyUpload(savePath, itemPath);
            }
            catch (Exception ex)
            {
                throw WrapTransferRelatedExceptions(ex);
            }
            
            if (!match) throw new FtpHashVerificationException(Resx.FtpHashVerificationFailed);
            return TransferResult.Ok;
        }

        //protected override string OpenCompressedFile(FileSystemItem item)
        //{
        //    var tempFilePath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), Guid.NewGuid());
        //    var fs = new FileStream(tempFilePath, FileMode.Create);
        //    FileManager.DownloadFile(item.Path, fs, 0, item.Size ?? 0);
        //    return tempFilePath;
        //}

        public TransferResult RemoteDownload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            UIThread.Run(() => eventAggregator.GetEvent<TransferActionStartedEvent>().Publish(ExportActionDescription));
            long resumeStartPosition = 0;
            try
            {
                switch (action)
                {
                    case CopyAction.CreateNew:
                        if (File.Exists(savePath)) throw new TransferException(TransferErrorType.WriteAccessError, item.Path, savePath, Resx.TargetAlreadyExists);
                        break;
                    case CopyAction.Overwrite:
                        File.Delete(savePath);
                        break;
                    case CopyAction.OverwriteOlder:
                        var fileDate = File.GetLastWriteTime(savePath);
                        if (fileDate > item.Date) return TransferResult.Skipped;
                        File.Delete(savePath);
                        break;
                    case CopyAction.Resume:
                        var fi = new FileInfo(savePath);
                        resumeStartPosition = fi.Length;
                        break;
                }

                var name = RemoteChangeDirectory(item.Path);
                Telnet.Download(name, savePath, item.Size ?? 0, resumeStartPosition, TelnetProgressChanged);
                return TransferResult.Ok;
            }
            catch (Exception ex)
            {
                throw WrapTransferRelatedExceptions(ex);
            }
        }

        public TransferResult RemoteUpload(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            UIThread.Run(() => eventAggregator.GetEvent<TransferActionStartedEvent>().Publish(ImportActionDescription));
            long resumeStartPosition = 0;
            try {
                switch (action)
                {
                    case CopyAction.CreateNew:
                        var exists = FileManager.FileExists(savePath);
                        if (exists) throw new TransferException(TransferErrorType.WriteAccessError, Resx.TargetAlreadyExists, item.Path, savePath, exists.Size);
                        break;
                    case CopyAction.Overwrite:
                        FileManager.DeleteFile(savePath);
                        break;
                    case CopyAction.OverwriteOlder:
                        var fileDate = FileManager.GetFileModificationTime(savePath);
                        if (fileDate > item.Date) return TransferResult.Skipped;
                        FileManager.DeleteFile(savePath);
                        break;
                    case CopyAction.Resume:
                        var fi = FileManager.GetItemInfo(savePath, ItemType.File);
                        resumeStartPosition = fi.Size ?? 0;
                        break;
                }
                var name = RemoteChangeDirectory(savePath);
                Telnet.Upload(item.Path, name, item.Size ?? 0, resumeStartPosition, TelnetProgressChanged);
                VerifyUpload(savePath, item.Path);
                return TransferResult.Ok;
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

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            if (CheckFreestyleDatabaseCommand != null) CheckFreestyleDatabaseCommand.RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            FileManager.Disconnect();
            _doContentScanOn.ForEach(TriggerContentScan);
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