using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NewConnectionDialog
    {
        public ConnectionImage[] ConnectionImages { get; private set; }

        public NewConnectionDialog(FtpConnectionItemViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            ConnectionImages = Enum.GetValues(typeof(ConnectionImage)).Cast<ConnectionImage>().ToArray();
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ConnectionName.Focus();
        }

        protected override void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (HasError()) return;
            base.OkButtonClick(sender, e);
        }

        private bool HasError()
        {
            var result = false;
            var controls = new[] {ConnectionName, Address, Port, Username, Password};
            foreach (var control in controls)
            {
                control.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                if (Validation.GetHasError(control)) result = true;
            }
            return result;
        }
    }
}