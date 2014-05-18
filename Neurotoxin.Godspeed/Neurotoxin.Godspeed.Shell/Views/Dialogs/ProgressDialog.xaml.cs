using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class ProgressDialog : IView<IProgressViewModel>
    {
        public IProgressViewModel ViewModel
        {
            get { return this.DataContext as IProgressViewModel; }
            set { this.DataContext = value; }
        }

        public ProgressDialog(IProgressViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            CloseButtonVisibility = Visibility.Collapsed;
            InitializeComponent();
            ViewModel = viewModel;
        }

    }
}