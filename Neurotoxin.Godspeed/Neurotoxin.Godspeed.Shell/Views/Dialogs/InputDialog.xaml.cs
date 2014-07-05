using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog
    {
        public static string ShowText(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options = null)
        {
            var viewModel = new InputDialogViewModel
            {
                Title = title,
                Message = message,
                DefaultValue = defaultValue,
                Options = options,
                Mode = InputDialogMode.ComboBox
            };
            var dialog = new InputDialog(viewModel);

            if (dialog.ShowDialog() == true)
            {
                var selectedItem = dialog.ComboBox.SelectedItem;
                if (selectedItem != null)
                {
                    var selectedValue = ((InputDialogOptionViewModel) dialog.ComboBox.SelectedItem);
                    if (selectedValue.DisplayName == dialog.ComboBox.Text) return (string)selectedValue.Value;
                }
                return dialog.ComboBox.Text;
            }
            return null;
        }

        public static object ShowList(string title, string message, object defaultValue, IList<InputDialogOptionViewModel> options)
        {
            if (options == null) throw new ArgumentException();

            var viewModel = new InputDialogViewModel
            {
                Title = title,
                Message = message,
                DefaultValue = defaultValue,
                Options = options,
                Mode = InputDialogMode.RadioGroup
            };

            var dialog = new InputDialog(viewModel);

            dialog.ShowDialog();
            return options.First(o => o.IsSelected).Value;
        }


        private InputDialog(InputDialogViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
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
            var vm = (InputDialogViewModel) DataContext;
            switch (vm.Mode)
            {
                case InputDialogMode.ComboBox:
                    ComboBox.Focus();
                    break;
                case InputDialogMode.RadioGroup:
                    UIThread.BeginRun(() =>
                    {
                        var option = vm.Options.FirstOrDefault(o => o.Equals(vm.DefaultValue)) ?? vm.Options.First();
                        option.IsSelected = true;
                    });
                    break;
            }
        }
    }
}