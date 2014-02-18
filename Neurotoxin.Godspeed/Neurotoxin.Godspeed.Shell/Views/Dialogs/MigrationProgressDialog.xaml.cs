﻿using System;
using System.Windows;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class MigrationProgressDialog
    {
        public const string TitleFormat = "Migrating... ({0}%)";

        public MigrationProgressDialog(CacheMigrationViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}