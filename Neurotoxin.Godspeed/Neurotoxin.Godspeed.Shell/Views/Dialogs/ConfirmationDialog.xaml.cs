using System;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ConfirmationDialog
    {
        public ConfirmationDialog(string title, string message)
        {
            Title = title;
            if (Application.Current.MainWindow != null) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = message;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
        }

    }
}