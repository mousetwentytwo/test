using System;
using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NewFolderDialog
    {
        public NewFolderDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}