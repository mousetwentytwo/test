using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;
using Neurotoxin.Contour.Modules.FileManager.Views.Commands;
using Neurotoxin.Contour.Modules.FileManager.Views.Dialogs;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.Views
{
    public partial class FileManagerView : ModuleViewBase
    {
        private TransferProgressDialog _transferProgressDialog;

        public new FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel)base.ViewModel; }
        }

        public FileManagerView(FileManagerViewModel viewModel)
        {
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
            if (_transferProgressDialog == null) _transferProgressDialog = new TransferProgressDialog(ViewModel);
            _transferProgressDialog.Show();
        }

        private void ViewModelOnTransferFinished()
        {
            if (_transferProgressDialog != null) _transferProgressDialog.Hide();
        }
    }
}