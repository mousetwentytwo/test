using System;
using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ReconnectionDialog
    {
        public ReconnectionDialog(Exception exception)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ErrorMessage.Text = exception.Message;
        }
    }
}