﻿using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Extensions;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class PaneView : ModuleViewBase
    {
        public const string DateTimeUiFormat = "dd/MM/yyyy HH:mm";

        public PaneView()
        {
            InitializeComponent();
            Grid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
            Grid.Sorting += GridOnSorting;
            Grid.Loaded += GridOnLoaded;
        }

        private void GridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var m = Grid.GetType().GetMethod("PerformSort", BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(Grid, new object[] { Grid.Columns[0] });
        }

        private void GridOnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
        }

        private void Grid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentRow = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (currentRow == null) return;
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void SetFocusToTheFirstCellOfCurrentRow()
        {
            if (Grid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                if (Grid.SelectedItem == null) return;
                var row = Grid.ItemContainerGenerator.ContainerFromItem(Grid.SelectedItem) as DataGridRow;
                if (row != null) row.FirstCell().Focus();
            }
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            SetFocusToTheFirstCellOfCurrentRow();
        }
    }
}