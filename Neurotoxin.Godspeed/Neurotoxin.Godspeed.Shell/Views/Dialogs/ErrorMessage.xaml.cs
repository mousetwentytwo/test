using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;
using System.Linq;
using Application = System.Windows.Application;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ErrorMessage
    {
        private readonly Exception _exception;

        public static void Show(Exception exception)
        {
            if (!UIThread.IsUIThread)
            {
                UIThread.Run(() => Show(exception));
                return;
            }

            var instance = new ErrorMessage(exception);
            instance.ShowDialog();
        }

        private ErrorMessage(Exception exception)
        {
            _exception = exception;
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = exception.Message;
            CallStack.Content = exception.StackTrace;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
            Loaded -= OnLoaded;
        }

        private void ReportButtonClick(object sender, RoutedEventArgs e)
        {
            const string boundary = "----GODSpeedFormBoundary";
            const string url = "http://www.mercenary.hu/godspeed/report.php";
            //const string url = "http://localhost/report.php";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "GODspeed";
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;

            var assembly = Assembly.GetAssembly(typeof(FileManagerWindow));
            var assemblyName = assembly.GetName();
            var appVersion = assemblyName.Version;

            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            var wpfVersion = fvi.ProductVersion;

            var os = Environment.OSVersion;

            try
            {
                var requestStream = request.GetRequestStream();
                var sw = new StreamWriter(requestStream);
                sw.WriteLine("--" + boundary);
                sw.WriteLine("Content-Disposition: form-data; name=\"log\"");
                sw.WriteLine();
                sw.WriteLine("GODspeed version: " + appVersion);
                sw.WriteLine("Framework version: " + wpfVersion);
                sw.WriteLine("OS version: " + os);
                sw.WriteLine(String.Empty);

                var ex = _exception;
                do
                {
                    sw.WriteLine("Error: " + ex.Message);
                    sw.WriteLine(ex.StackTrace);
                    sw.WriteLine(String.Empty);
                    ex = ex.InnerException;
                } 
                while (ex != null); 


                var w = Application.Current.MainWindow as FileManagerWindow;
                if (w != null)
                {
                    var ftp = w.ViewModel.RightPane as FtpContentViewModel;
                    if (ftp == null)
                    {
                        var connections = w.ViewModel.RightPane as ConnectionsViewModel;
                        if (connections != null) ftp = connections.ConnectedFtp;
                    }
                    if (ftp != null)
                    {
                        for (var i = ftp.Log.Count - 1; i >= 0; i--)
                        {
                            sw.WriteLine(ftp.Log.ElementAt(i));
                        }
                    }
                }

                var iw = 0;
                foreach (Window window in Application.Current.Windows)
                {
                    sw.WriteLine("--" + boundary);
                    sw.WriteLine("Content-Disposition: form-data; name=\"window{0}\"; filename=\"{0}.png\";", iw);
                    sw.WriteLine("Content-Type: image/png");
                    sw.WriteLine();
                    sw.Flush();
                    var ms = window.CaptureVisual();
                    ms.Position = 0;
                    ms.CopyTo(requestStream);
                    requestStream.Flush();
                    ms.Dispose();
                    sw.WriteLine();
                    iw++;
                }
                sw.WriteLine("--" + boundary + "--");
                sw.Flush();
                request.GetResponse();
            }
            catch
            {
                
            }
            OkButtonClick(sender, e);
        }

    }
}