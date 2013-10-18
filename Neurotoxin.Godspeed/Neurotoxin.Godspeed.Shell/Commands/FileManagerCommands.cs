using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Views;

namespace Neurotoxin.Godspeed.Shell.Commands
{
    public static class FileManagerCommands
    {
        public static readonly RoutedUICommand OpenDriveDropdownCommand = new RoutedUICommand("Open Drive Dropdown", "OpenDriveDropdown", typeof(FileManagerView));
    }
}