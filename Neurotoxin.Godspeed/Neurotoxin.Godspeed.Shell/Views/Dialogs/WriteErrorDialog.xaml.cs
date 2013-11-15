using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class WriteErrorDialog : ITransferErrorDialog
    {
        private readonly TransferException _exception;
        private readonly IEventAggregator _eventAggregator;
        public TransferErrorDialogResult Result { get; private set; }

        public WriteErrorDialog(IFileListPaneViewModel viewModel, TransferException exception, IEventAggregator eventAggregator)
        {
            _exception = exception;
            _eventAggregator = eventAggregator;
            Owner = Application.Current.MainWindow;
            DataContext = viewModel;
            InitializeComponent();
            _eventAggregator.GetEvent<ViewModelGeneratedEvent>().Subscribe(ViewModelGenerated);
        }

        private void ViewModelGenerated(ViewModelGeneratedEventArgs args)
        {
            var vm = args.ViewModel as FileSystemItemViewModel;
            if (vm == null) return;
            if (vm.Path == _exception.SourceFile) SourceFile.DataContext = vm;
            if (vm.Path == _exception.TargetFile) TargetFile.DataContext = vm;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Overwrite":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Overwrite);
                    break;
                case "OverwriteAll":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.Overwrite);
                    break;
                case "OverwriteAllOlder":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.OverwriteOlder);
                    break;
                case "Resume":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.Current, CopyAction.Resume);
                    break;
                case "ResumeAll":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Retry, CopyActionScope.All, CopyAction.Resume);
                    break;
                case "Rename":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Rename);
                    break;
                case "Skip":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip);
                    break;
                case "SkipAll":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Skip, CopyActionScope.All);
                    break;
                case "Cancel":
                    Result = new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
                    break;
            }
            DialogResult = true;
            Close();
            _eventAggregator.GetEvent<ViewModelGeneratedEvent>().Unsubscribe(ViewModelGenerated);
        }
    }
}