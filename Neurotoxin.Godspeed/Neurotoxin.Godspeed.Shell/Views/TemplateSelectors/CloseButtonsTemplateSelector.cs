﻿using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.TemplateSelectors
{
    public class CloseButtonsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FtpTemplate { get; set; }
        public DataTemplate StfsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object viewModel, DependencyObject container)
        {
            if (viewModel is FtpContentViewModel) return FtpTemplate;
            if (viewModel is StfsPackageContentViewModel) return StfsTemplate;
            return DefaultTemplate;
        }
    }
}
