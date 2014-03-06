﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Application = System.Windows.Application;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ErrorMessage
    {
        private readonly string _details;
        private readonly string _ftpLog;

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
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = exception.Message;

            var sb = new StringBuilder();
            var ex = exception is SomethingWentWrongException ? exception.InnerException : exception;
            do
            {
                sb.AppendLine("Error: " + ex.Message);
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine(String.Empty);
                ex = ex.InnerException;
            }
            while (ex != null);
            _details = sb.ToString();
            Details.Content = _details;

            _ftpLog = GetFtpLog();
            FtpLog.Content = _ftpLog;
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
                        Details = _details,
                        FtpLog = _ftpLog
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

        private static string GetFtpLog()
        {
            var sb = new StringBuilder();
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
                        sb.Append(ftp.Log.ElementAt(i));
                    }
                    return sb.ToString();
                }
            }
            return null;
        }

    }
}