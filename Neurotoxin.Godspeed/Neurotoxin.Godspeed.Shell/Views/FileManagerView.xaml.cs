using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileManagerView : Window
    {
        private TransferProgressDialog _transferProgressDialog;

        public FileManagerView(FileManagerViewModel viewModel)
        {
            var assembly = Assembly.GetAssembly(typeof(FileManagerView));
            var assemblyName = assembly.GetName();
            var version = assemblyName.Version;
            Title = String.Format("GODspeed v{0}", version);

            InitializeComponent();
            DataContext = viewModel;
            viewModel.TransferStarted += ViewModelOnTransferStarted;
            viewModel.TransferFinished += ViewModelOnTransferFinished;
            CommandBindings.Add(new CommandBinding(FileManagerCommands.OpenDriveDropdownCommand, ExecutedOpenDriveDropdownCommand));
        }

        private void ExecutedOpenDriveDropdownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var control = e.Parameter as ContentControl;
            if (control == null) return;
            var pane = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(control, 0), 0) as FileListPane;
            if (pane == null) return;
            var combobox = pane.FindName("DriveDropdown") as ComboBox;
            if (combobox == null) return;

            combobox.IsDropDownOpen = true;
            var item = combobox.ItemContainerGenerator.ContainerFromItem(combobox.SelectedItem) as ComboBoxItem;
            item.Focus();
        }

        private void ViewModelOnTransferStarted()
        {
            if (_transferProgressDialog == null) _transferProgressDialog = new TransferProgressDialog((FileManagerViewModel)DataContext);
            _transferProgressDialog.Show();
        }

        private void ViewModelOnTransferFinished()
        {
            if (_transferProgressDialog != null) _transferProgressDialog.Hide();
        }
    }
}