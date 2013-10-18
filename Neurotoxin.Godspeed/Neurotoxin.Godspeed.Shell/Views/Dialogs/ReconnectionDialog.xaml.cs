using System;
using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ReconnectionDialog : Window
    {
        public ReconnectionDialog(Exception exception)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ErrorMessage.Text = exception.Message;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Reconnect":
                    DialogResult = true;
                    break;
                case "Cancel":
                    DialogResult = false;
                    break;
            }
            Close();
        }
    }
}