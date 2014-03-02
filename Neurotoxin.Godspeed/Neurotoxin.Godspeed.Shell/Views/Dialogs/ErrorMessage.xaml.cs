using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
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
            var formData = new List<IFormData>
                {
                    new ErrorReport
                    {
                        Name = "log",
                        ClientId = EsentPersistentDictionary.Instance.Get<string>("ClientID"),
                        ApplicationVersion = App.GetApplicationVersion(),
                        FrameworkVersion = App.GetFrameworkVersion(),
                        OperatingSystemVersion = Environment.OSVersion.VersionString,
                        Exception = _exception,
                        FtpLog = GetFtpLog()
                    }
                };
            var iw = 0;
            foreach (Window window in Application.Current.Windows)
            {
                formData.Add(new WindowScreenshotReport
                {
                    Name = "window" + iw,
                    Window = window
                });
                iw++;
            }

            HttpForm.Post("http://www.mercenary.hu/godspeed/report.php", formData);
            OkButtonClick(sender, e);
        }

        private static Stack<string> GetFtpLog()
        {
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
                    return ftp.Log;
                }
            }
            return null;
        }

    }
}