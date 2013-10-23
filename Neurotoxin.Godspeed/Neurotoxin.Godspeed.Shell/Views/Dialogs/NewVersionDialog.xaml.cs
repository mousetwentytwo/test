using System;
using System.Diagnostics;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NewVersionDialog
    {
        public NewVersionDialog(string message)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = message;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Download.Focus();
        }

        private void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start("http://godspeed.codeplex.com");
            OkButtonClick(sender, e);
        }


    }
}