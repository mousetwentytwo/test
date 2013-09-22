using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class FtpBrowserView : ModuleViewBase
    {
        public const string DateTimeUiFormat = "dd/MM/yyyy HH:mm";

        public new FtpBrowserViewModel ViewModel
        {
            get { return (FtpBrowserViewModel)base.ViewModel; }
        }

        public FtpBrowserView(FtpBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            this.Loaded += View_Loaded;
        }

        void View_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override bool Close()
        {
            //FtpBrowserViewModel viewModel = (FtpBrowserViewModel)ViewModel;
            //if (viewModel.KeepDirty()) return false;
            //viewModel.ResetChanges();
            return base.Close();
        }
    }
}