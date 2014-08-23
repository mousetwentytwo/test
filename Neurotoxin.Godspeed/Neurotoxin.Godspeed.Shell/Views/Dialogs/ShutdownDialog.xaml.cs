using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ShutdownDialog : IView<ShutdownDialogViewModel>
    {
        public ShutdownDialogViewModel ViewModel
        {
            get { return DataContext as ShutdownDialogViewModel; }
        }

        public ShutdownDialog(ShutdownDialogViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ShutdownNowButton.IsVisible) ShutdownNowButton.Focus();
            if (ShutdownNowSplitButton.IsVisible) ShutdownNowSplitButton.Focus();
        }
    }
}