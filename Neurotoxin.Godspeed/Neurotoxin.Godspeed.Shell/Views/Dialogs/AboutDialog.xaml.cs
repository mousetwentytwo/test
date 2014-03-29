using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            Owner = Application.Current.MainWindow;
            Title = Owner.Title;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Ok.Focus();
        }

    }
}