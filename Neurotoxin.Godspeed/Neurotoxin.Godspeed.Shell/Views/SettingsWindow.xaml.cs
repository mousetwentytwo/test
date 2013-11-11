using System;
using System.Windows;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel) DataContext).SaveChanges();
            base.OkButtonClick(sender, e);
        }
    }
}