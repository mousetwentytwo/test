using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
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

        public bool IsConnected
        {
            get { return FileManager.IsConnected; }
        }

        public bool IsKeepAliveEnabled
        {
            get { return FileManager.IsKeepAliveEnabled; }
            set { FileManager.IsKeepAliveEnabled = value; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        private bool IsContentScanTriggerAvailable
        {
            get
            {
                return UserSettingsProvider.FsdContentScanTrigger != FsdContentScanTrigger.Disabled &&
                       Connection != null && !Connection.IsHttpAccessDisabled && FileManager.IsFSD;
            }
        }

        public override bool IsFSD
        {
            get { return FileManager.IsFSD; }
        }

        public override bool IsVerificationEnabled
        {
            get { return UserSettingsProvider.VerifyFileHashAfterFtpUpload && FileManager.IsFSD; }
        }

        #region Command overrides

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, Connection));
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
            if (WindowManager.ActivateWindowOf<FreestyleDatabaseCheckerViewModel>()) return;
            var vm = Container.Resolve<FreestyleDatabaseCheckerViewModel>(new ParameterOverride("parent", this));
            vm.Check();
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

        #region ShowFtpLogCommand

        public DelegateCommand ShowFtpLogCommand { get; private set; }

        public void ExecuteShowFtpLogCommand()
        {
            EventAggregator.GetEvent<ShowFtpTraceWindowEvent>().Publish(new ShowFtpTraceWindowEventArgs(FileManager.TraceListener, Connection.Name));
        }

        #endregion

        public FtpContentViewModel()
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            CheckFreestyleDatabaseCommand = new DelegateCommand(ExecuteCheckFreestyleDatabaseCommand, CanExecuteCheckFreestyleDatabaseCommand);
            LaunchGameCommand = new DelegateCommand(ExecuteLaunchGameCommand, CanExecuteLaunchGameCommand);
            LaunchXexCommand = new DelegateCommand(ExecuteLaunchXexCommand, CanExecuteLaunchXexCommand);
            ShowFtpLogCommand = new DelegateCommand(ExecuteShowFtpLogCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkHandler.Run(
                        () =>
                            {
                                Connection = (FtpConnectionItemViewModel)cmdParam.Payload;
                                return Connect();
                            },
                        result =>
                            {
                                IsLoaded = true;
                                ResumeCapability = result;
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
                    WorkHandler.Run(
                        () =>
                            {
                                //TODO: upload binary
                                //File.WriteAllBytes(payload.TempFilePath, payload.Content);
                                //FileManager.RestoreConnection();
                                //FileManager.UploadFile(payload.FilePath, payload.TempFilePath);
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

        private ResumeCapability Connect()
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
                                    UserSettingsProvider.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
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
                                    EventAggregator.GetEvent<ConnectionDetailsChangedEvent>().Publish(new ConnectionDetailsChangedEventArgs(Connection));
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
                                UserSettingsProvider.FsdContentScanTrigger = FsdContentScanTrigger.Disabled;
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

        public void Shutdown()
        {
            FileManager.Shutdown();
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

            switch (UserSettingsProvider.FsdContentScanTrigger)
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

        public override IList<FileSystemItem> GetList(string selectedPath)
        {
            return FileManager.ServerType == FtpServerType.PlayStation3
                ? FileManager.GetList(selectedPath)
                : base.GetList(selectedPath);
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

        protected override void AsyncErrorCallback(Exception ex)
        {
            base.AsyncErrorCallback(WrapFtpException(ex));
        }

        public override TransferResult CreateFolder(string path)
        {
            try
            {
                CheckPathForSpecialChars(path); 
                return base.CreateFolder(path);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override TransferResult Delete(FileSystemItem item)
        {
            try
            {
                return base.Delete(item);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            try
            {
                CheckPathForSpecialChars(path);
                return base.GetStream(path, mode, access, startPosition);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        public override bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null)
        {
            try
            {
                return base.CopyStream(item, stream, startPosition, byteLimit);
            }
            catch (Exception ex)
            {
                throw WrapFtpException(ex);
            }
        }

        private void CheckPathForSpecialChars(string path)
        {
            //TODO: error message might be not ok
            if (new Regex(@"[^\x20-\x7f]").IsMatch(path))
                throw new TransferException(TransferErrorType.NotSupporterCharactersInPath, Resx.SpecialCharactersNotSupported);
        }

        private Exception WrapFtpException(Exception exception)
        {
            if (FileManager.IsConnected) return exception;
            return new TransferException(TransferErrorType.LostConnection, string.Format(Resx.ConnectionLostMessage, Connection.Name), exception)
                       {
                           Pane = this
                       };
        }

        public TransferResult Verify(string remotePath, string localPath)
        {
            if (!FileManager.IsFSD) return TransferResult.Skipped;

            var match = FileManager.Verify(remotePath, localPath);
            if (!match) throw new FtpHashVerificationException(Resx.FtpHashVerificationFailed);
            return TransferResult.Ok;
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            if (CheckFreestyleDatabaseCommand != null) CheckFreestyleDatabaseCommand.RaiseCanExecuteChanged();
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<CloseFtpTraceWindowEvent>().Publish(new CloseFtpTraceWindowEventArgs(FileManager.TraceListener));
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