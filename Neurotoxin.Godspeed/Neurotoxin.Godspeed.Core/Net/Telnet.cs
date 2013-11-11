using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Neurotoxin.Godspeed.Core.Exceptions;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Net
{
    public static class Telnet
    {
        private static TcpClient _client;
        private static NetworkStream _ns;
        private static string _rootPath;
        private static readonly Regex ShareParser = new Regex(@"^\\\\(?<host>.*?)\\(?<sharename>.*?)(?<directory>\\.*)?$");
        private static readonly Regex RootPathParser = new Regex(@"path\s*=\s*(?<path>.*)$", RegexOptions.IgnoreCase);
        private static readonly Regex ProgressParser = new Regex(@"' at (?<totalTransferred>[0-9]+) \((?<percentage>[0-9]+)%\)");
        private static readonly Regex FinishParser = new Regex(@"(?<totalTransferred>[0-9]+) bytes transferred in");
        private static long _totalTransferred = 0;

        public static void OpenSession(string sambaShare, string ftpHost, int port, string ftpUser, string ftpPwd)
        {
            if (_client != null) throw new TelnetException(null, "A telnet session is already opened. Please close the current one first before starting a new one.");
            
            var shareParts = ShareParser.Match(sambaShare);
            if (!shareParts.Success)
                throw new TelnetException("?", "Invalid UNC path: {0}", sambaShare);
            var host = shareParts.Groups["host"].Value;
            var shareName = shareParts.Groups["sharename"].Value;
            var directory = shareParts.Groups["directory"].Value;

            _client = Connect(host);
            _ns = _client.GetStream();

            _rootPath = null;

            Send(string.Format("sed -e '/{0}/,/path/!d' /etc/samba/smb.conf", shareName), s =>
                {
                    var m = RootPathParser.Match(s);
                    if (m.Success)
                    {
                        _rootPath = m.Groups["path"].Value;
                        if (!string.IsNullOrEmpty(directory))
                        {
                            _rootPath = _rootPath.TrimEnd('/') + directory.Replace(@"\", "/");
                        }
                        if (!_rootPath.EndsWith("/")) _rootPath += "/";
                    }
                });
            if (string.IsNullOrEmpty(_rootPath))
                throw new TelnetException(host, "Samba share not found");

            Send("lftp");
            Send(string.Format("open -p {0} {1}", port, ftpHost));
            Send(string.Format("user {0} {1}", ftpUser, ftpPwd));
        }

        private static TcpClient Connect(string host)
        {
            var timeoutObject = new ManualResetEvent(false);
            var connected = false;
            Exception exception = null;
            var client = new TcpClient();
            client.BeginConnect(host, 23, ar =>
                {
                    try
                    {
                        connected = false;
                        if (client.Client != null)
                        {
                            client.EndConnect(ar);
                            connected = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        connected = false;
                        exception = ex;
                    }
                    finally
                    {
                        timeoutObject.Set();
                    }
                }, null);

            if (timeoutObject.WaitOne(5000, false))
            {
                if (connected) return client;
                throw new TelnetException(host, exception, "Connection failed. See inner exception for details.");
            }

            client.Close();
            throw new TelnetException(host, "Connection timeout");
        }

        public static void CloseSession()
        {
            Send("exit");
            Send("exit");
            _ns = null;
            _client = null;
        }

        public static void ChangeFtpDirectory(string path)
        {
            Send(string.Format("cd {0}", path));
        }

        public static void Download(string ftpFileName, string targetPath, bool continueOrReget, Action<int, long, long> progressChanged)
        {
            //TODO: use c
            Send(string.Format("get -O {0} {1} -o {2}", _rootPath, ftpFileName, targetPath), s => NotifyProgressChange(s, progressChanged));
        }

        public static void Upload(string sourcePath, string ftpFileName, bool continueOrReput, Action<int, long, long> progressChanged)
        {
            //TODO: use c
            Send(string.Format("put -O {0} {1} -o {2}", _rootPath, sourcePath, ftpFileName), s => NotifyProgressChange(s, progressChanged));
        }

        private static void NotifyProgressChange(string s, Action<int, long, long> progressChanged)
        {
            if (string.IsNullOrEmpty(s.Trim())) return;

            var m = ProgressParser.Matches(s).Cast<Match>().LastOrDefault();
            if (m != null)
            {
                var percentage = Int32.Parse(m.Groups["percentage"].Value);
                var t = Int32.Parse(m.Groups["totalTransferred"].Value);
                var transferred = _totalTransferred - _totalTransferred;
                _totalTransferred = t;
                progressChanged.Invoke(percentage, transferred, _totalTransferred);
            }
            else
            {
                m = FinishParser.Match(s);
                if (m.Success)
                {
                    var t = Int32.Parse(m.Groups["totalTransferred"].Value);
                    var transferred = _totalTransferred - _totalTransferred;
                    _totalTransferred = t;
                    progressChanged.Invoke(100, transferred, _totalTransferred);
                }
            }
        }

        private static string Read()
        {
            var sb = new StringBuilder();
            if (_ns.CanRead)
            {
                var readBuffer = new byte[1024];
                while (_ns.DataAvailable)
                {
                    var numBytesRead = _ns.Read(readBuffer, 0, readBuffer.Length);
                    var data = Encoding.ASCII.GetString(readBuffer, 0, numBytesRead);
                    data = data.Replace(Convert.ToChar(24), ' ');
                    data = data.Replace(Convert.ToChar(255), ' ');
                    data = data.Replace('?', ' ');
                    sb.AppendFormat("{0}", data);
                }
            }
            Thread.Sleep(100);
            return sb.ToString();
        }

        private static void Send(string message, Action<string> processResponse = null)
        {
            var msg = Encoding.ASCII.GetBytes(message + Environment.NewLine);
            _ns.Write(msg, 0, msg.Length);
            string x;
            do
            {
                x = Read();
                if (processResponse != null) processResponse.Invoke(x);
            } 
            while (!x.EndsWith("> "));
        }
    }
}