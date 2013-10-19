using System;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog
    {
        public InputDialog(string title, string message, string defaultValue)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Title = title;
            Message.Text = message;
            Input.Text = defaultValue;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Input.Focus();
        }
    }
}