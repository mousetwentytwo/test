using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog
    {
        public static string Show(string title, string message, string defaultValue, IEnumerable<InputDialogOptionViewModel> options = null)
        {
            var dialog = new InputDialog(title, message, defaultValue, options);
            return dialog.ShowDialog() == true
                       ? dialog.Input.SelectedItem != null
                             ? ((InputDialogOptionViewModel)dialog.Input.SelectedItem).Value
                             : dialog.Input.Text
                       : null;
        }

        private InputDialog(string title, string message, string defaultValue, IEnumerable<InputDialogOptionViewModel> options = null)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Title = title;
            Message.Text = message;
            Input.Text = defaultValue;
            Input.ItemsSource = options;
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