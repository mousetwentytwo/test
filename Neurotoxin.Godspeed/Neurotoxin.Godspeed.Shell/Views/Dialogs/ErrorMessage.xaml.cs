﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ErrorMessage
    {
        private Exception _exception;

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
            var request = (HttpWebRequest)WebRequest.Create("http://www.mercenary.hu/godspeed/report.php");
            request.UserAgent = "GODspeed";
            request.Method = "POST";

            var assembly = Assembly.GetAssembly(typeof(FileManagerWindow));
            var assemblyName = assembly.GetName();
            var appVersion = assemblyName.Version;

            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            var wpfVersion = fvi.ProductVersion;

            var os = Environment.OSVersion;

            try
            {
                var sw = new StreamWriter(request.GetRequestStream());
                sw.WriteLine("GODspeed version: " + appVersion);
                sw.WriteLine("Framework version: " + wpfVersion);
                sw.WriteLine("OS version: " + os);
                sw.WriteLine(String.Empty);

                var ex = _exception;
                do
                {
                    sw.WriteLine("Error: " + _exception.Message);
                    sw.WriteLine(_exception.StackTrace);
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

                sw.Flush();
                request.GetResponse();
            }
            catch {}
            OkButtonClick(sender, e);
        }

    }
}