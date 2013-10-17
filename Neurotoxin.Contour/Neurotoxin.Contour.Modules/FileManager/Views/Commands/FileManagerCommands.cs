using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Commands
{
    public static class FileManagerCommands
    {
        public static readonly RoutedUICommand OpenDriveDropdownCommand = new RoutedUICommand("Open Drive Dropdown", "OpenDriveDropdown", typeof(FileManagerView));
    }
}