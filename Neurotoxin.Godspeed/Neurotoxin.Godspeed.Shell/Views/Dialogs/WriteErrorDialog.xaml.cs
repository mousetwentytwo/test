﻿using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class WriteErrorDialog : ITransferErrorDialog
    {
        private readonly string _sourceFile;
        private readonly string _targetFile;
        private readonly IEventAggregator _eventAggregator;
        public TransferErrorDialogResult Result { get; private set; }

        public WriteErrorDialog(IEventAggregator eventAggregator, string sourceFile, string targetFile, CopyAction disableFlags)
        {
            _eventAggregator = eventAggregator;
            _sourceFile = sourceFile;
            _targetFile = targetFile;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            if (string.IsNullOrEmpty(sourceFile)) SourceFileBox.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(targetFile)) TargetFileBox.Visibility = Visibility.Collapsed;
            DisableButtons(disableFlags);
            _eventAggregator.GetEvent<ViewModelGeneratedEvent>().Subscribe(ViewModelGenerated);
        }

        private void DisableButtons(CopyAction disableFlags)
        {
            if (disableFlags.HasFlag(CopyAction.Overwrite))
            {
                Overwrite.IsEnabled = false;
                OverwriteAll.IsEnabled = false;
                OverwriteAllOlder.IsEnabled = false;
            }
            if (disableFlags.HasFlag(CopyAction.OverwriteOlder))
            {
                OverwriteAllOlder.IsEnabled = false;
            }
            if (disableFlags.HasFlag(CopyAction.Resume))
            {
                Resume.IsEnabled = false;
                ResumeAll.IsEnabled = false;
            }
            if (disableFlags.HasFlag(CopyAction.Rename))
            {
                Rename.IsEnabled = false;
            }
        }

        private void ViewModelGenerated(ViewModelGeneratedEventArgs args)
        {
            var vm = args.ViewModel as FileSystemItemViewModel;
            if (vm == null) return;
            if (vm.Path == _sourceFile) SourceFile.DataContext = vm;
            if (vm.Path == _targetFile) TargetFile.DataContext = vm;
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