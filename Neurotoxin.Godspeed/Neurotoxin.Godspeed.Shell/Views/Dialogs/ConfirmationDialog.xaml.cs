using System;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ConfirmationDialog
    {
        public ConfirmationDialog(string message)
        {
            Owner = Application.Current.MainWindow;
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