using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NewFolderDialog
    {
        public NewFolderDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FolderName.Focus();
        }

    }
}