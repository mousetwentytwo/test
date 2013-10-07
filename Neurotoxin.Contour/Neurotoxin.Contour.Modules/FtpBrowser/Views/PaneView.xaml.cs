using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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