using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileManagerWindow
    {
        private bool _isAbortionInProgress;
        private TransferProgressDialog _transferProgressDialog;

        private FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel) DataContext; }
        }

        public FileManagerWindow(FileManagerViewModel viewModel)
        {
            var assembly = Assembly.GetAssembly(typeof(FileManagerWindow));
            var assemblyName = assembly.GetName();
            var version = assemblyName.Version;
            Title = String.Format("GODspeed v{0}", version);

            InitializeComponent();
            DataContext = viewModel;
            viewModel.TransferStarted += ViewModelOnTransferStarted;
            viewModel.TransferFinished += ViewModelOnTransferFinished;
            CommandBindings.Add(new CommandBinding(FileManagerCommands.OpenDriveDropdownCommand, ExecutedOpenDriveDropdownCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.SettingsCommand, ExecutedSettingsCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.AboutCommand, ExecutedAboutCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.ExitCommand, ExecuteExitCommand));

            LayoutRoot.PreviewKeyDown += LayoutRootOnPreviewKeyDown;
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            ViewModel.Dispose();
        }

        //HACK: Temporary solution. KeyBinding doesn't work with Key.Delete (requires investigation)
        private void LayoutRootOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            var deleteCommand = ViewModel.DeleteCommand;
            if (!deleteCommand.CanExecute()) return;
            e.Handled = true;
            deleteCommand.Execute();
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

        private void ExecutedSettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var settings = UnityInstance.Container.Resolve<SettingsWindow>();
            settings.ShowDialog();
        }

        private void ExecutedAboutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void ExecuteExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ViewModelOnTransferStarted()
        {
            if (_transferProgressDialog == null)
            {
                _transferProgressDialog = new TransferProgressDialog(ViewModel);
                _transferProgressDialog.Closing += TransferProgressDialogOnClosing;
                _transferProgressDialog.Closed += TransferProgressDialogOnClosed;
            }
            IsHitTestVisible = false;
            _transferProgressDialog.Show();
        }

        private void TransferProgressDialogOnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (_isAbortionInProgress) return;
            if (!ViewModel.TargetPane.IsResumeSupported)
            {
                var d = new ConfirmationDialog("Warning", "The resume function on target machine is not available. Do you really want to abort the transfer?");
                if (d.ShowDialog() != true) return;
            }
            _transferProgressDialog.Abort.IsEnabled = false;
            _isAbortionInProgress = true;
            ViewModel.AbortTransfer();
        }

        private void TransferProgressDialogOnClosed(object sender, EventArgs e)
        {
            IsHitTestVisible = true;
            _transferProgressDialog.Closed -= TransferProgressDialogOnClosed;
            _transferProgressDialog = null;
        }

        private void ViewModelOnTransferFinished()
        {
            if (_transferProgressDialog == null) return;
            _transferProgressDialog.Closing -= TransferProgressDialogOnClosing;
            _transferProgressDialog.Close();
        }

    }
}