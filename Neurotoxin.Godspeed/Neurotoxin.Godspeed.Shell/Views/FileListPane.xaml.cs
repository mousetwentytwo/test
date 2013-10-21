using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileListPane : UserControl
    {
        public const string DateTimeUiFormat = "dd/MM/yyyy HH:mm";

        public FileListPane()
        {
            InitializeComponent();
            var eventAggregator = UnityInstance.Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(ActivePaneChanged);

            Grid.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
            Grid.Sorting += GridOnSorting;
            Grid.Loaded += GridOnLoaded;
            Grid.SelectionChanged += GridOnSelectionChanged;
        }

        private void ActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            if (DataContext != e.ActivePane) return;
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void GridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var m = Grid.GetType().GetMethod("PerformSort", BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(Grid, new object[] { Grid.Columns[0] });
            Grid.Loaded -= GridOnLoaded;
        }

        private void GridOnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
        }

        private void GridOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentRow = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (currentRow == null) return;
            Grid.ScrollIntoView(currentRow);
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void SetFocusToTheFirstCellOfCurrentRow()
        {
            if (Grid.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            if (Grid.SelectedItem == null) return;
            var row = Grid.ItemContainerGenerator.ContainerFromItem(Grid.SelectedItem) as DataGridRow;
            if (row == null) return;
            try
            {
                row.FirstCell().Focus();
            } 
            catch {}
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            SetFocusToTheFirstCellOfCurrentRow();
        }

        private void TitleEditBoxLoaded(object sender, RoutedEventArgs e)
        {
            var box = (TextBox) sender;
            box.SelectAll();
            box.Focus();
            box.ScrollToVerticalOffset(10);
        }
    }
}