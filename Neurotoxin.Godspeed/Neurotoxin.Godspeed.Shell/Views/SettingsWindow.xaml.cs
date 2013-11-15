using System;
using System.Text;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            var desc = ApplicationExtensions.GetContentByteArray("/Resources/UseRemoteCopyRemarks.txt");
            UseRemoteCopyRemarks.Text = Encoding.UTF8.GetString(desc);
            DataContext = viewModel;
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel) DataContext).SaveChanges();
            base.OkButtonClick(sender, e);
        }
    }
}