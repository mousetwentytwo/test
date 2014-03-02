using System.Text;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Formatters;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            Owner = Application.Current.MainWindow;
            Title = Owner.Title;
            InitializeComponent();
            var desc = ApplicationExtensions.GetContentByteArray("/Resources/About.html");
            Description.TextFormatter = new HtmlFormatter();
            Description.Text = Encoding.UTF8.GetString(desc);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Ok.Focus();
        }

    }
}