using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Extensions;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class PaneView : ModuleViewBase
    {
        public PaneView()
        {
            InitializeComponent();
        }

        private void Grid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentRow = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (currentRow == null) return;
            Grid.ItemContainerGenerator.StatusChanged += SetFocusToTheFirstCellOfCurrentRowWhenReady;
        }

        private void SetFocusToTheFirstCellOfCurrentRowWhenReady(object sender, EventArgs e)
        {
            var generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) return;
            var row = generator.ContainerFromItem(Grid.SelectedItem) as DataGridRow;
            row.FirstCell().Focus();
            generator.StatusChanged -= SetFocusToTheFirstCellOfCurrentRowWhenReady;
        }
    }
}