using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FreestyleDatabaseCheckerWindow
    {
        public static void Show(FtpContentViewModel parent)
        {
            if (!UIThread.IsUIThread)
            {
                UIThread.BeginRun(() => Show(parent));
                return;
            }

            var instance = new FreestyleDatabaseCheckerWindow(new FreestyleDatabaseCheckerViewModel(parent));
            instance.ShowDialog();
        }

        public FreestyleDatabaseCheckerWindow(FreestyleDatabaseCheckerViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}