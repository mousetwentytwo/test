using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Views
{
    public class PaneTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate FileListTemplate { get; set; }
        public DataTemplate ConnectionsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ConnectionsViewModel) return ConnectionsTemplate;
            if (item is IFileListPaneViewModel) return FileListTemplate;
            return DefaultTemplate;
        }
    }
}
