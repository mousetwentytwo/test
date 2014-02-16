using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.Views.Selectors
{
    public class CellEditingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TitleTemplate { get; set; }
        public DataTemplate NameTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cell = ((FrameworkElement)container).FindAncestor<DataGridCell>();
            var tag = cell.Tag as string;
            switch (tag)
            {
                case "Name":
                    return NameTemplate;
                default:
                    return TitleTemplate;
            }
        }
    }
}