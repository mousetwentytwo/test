using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FreestyleDatabaseCheckerWindow : IView<FreestyleDatabaseCheckerViewModel>
    {
        public FreestyleDatabaseCheckerViewModel ViewModel
        {
            get { return this.DataContext as FreestyleDatabaseCheckerViewModel; }
            private set { this.DataContext = value; }
        }

        public FreestyleDatabaseCheckerWindow(FreestyleDatabaseCheckerViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel = viewModel;
        }
    }
}