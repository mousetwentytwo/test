using System;
using System.Windows;
using System.Windows.Input;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog
    {
        public InputDialog(string title, string message, string defaultValue)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Title = title;
            Message.Text = message;
            Input.Text = defaultValue;
            Loaded += OnLoaded;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            DialogResult = true;
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Input.Focus();
        }
    }
}