﻿using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class NewConnectionDialog : Window
    {
        public ConnectionImage[] ConnectionImages { get; private set; }

        public NewConnectionDialog(FtpConnectionItemViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            ConnectionImages = Enum.GetValues(typeof(ConnectionImage)).Cast<ConnectionImage>().ToArray();
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (HasError()) return;
            DialogResult = true;
            Close();
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

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}