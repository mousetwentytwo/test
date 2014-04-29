using System;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class LoginDialog
    {
        public static LoginDialogResult Show(string title, string message, string defaultUsername, string defaultPassword)
        {
            var dialog = new LoginDialog(title, message, defaultUsername, defaultPassword);
            return dialog.ShowDialog() == true
                ? new LoginDialogResult
                {
                    Username = dialog.Username.Text,
                    Password = dialog.Password.Text,
                    RememberPassword = dialog.RememberPassword.IsChecked == true
                }
                : null;
        }

        private LoginDialog(string title, string message, string defaultUsername, string defaultPassword)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Title = title;
            Message.Text = message;
            Username.Text = defaultUsername;
            Password.Text = defaultPassword;
            Loaded += OnLoaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            DialogResult = true;
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Username.Focus();
            Loaded -= OnLoaded;
        }
    }
}