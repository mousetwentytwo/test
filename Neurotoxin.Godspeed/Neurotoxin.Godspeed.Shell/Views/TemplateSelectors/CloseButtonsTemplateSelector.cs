using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.TemplateSelectors
{
    public class CloseButtonsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FtpTemplate { get; set; }
        public DataTemplate PackageTemplate { get; set; }
        public DataTemplate CompressedFileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object viewModel, DependencyObject container)
        {
            if (viewModel is FtpContentViewModel) return FtpTemplate;
            if (viewModel is StfsPackageContentViewModel) return PackageTemplate;
            if (viewModel is CompressedFileContentViewModel) return CompressedFileTemplate;
            return DefaultTemplate;
        }
    }
}
