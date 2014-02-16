using System;
using System.Text;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class StatisticsWindow
    {
        public const string TimeFormat = "{0:hh\\:mm\\:ss}";

        public StatisticsWindow(StatisticsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}