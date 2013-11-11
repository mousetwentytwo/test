﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Views;

namespace Neurotoxin.Godspeed.Shell.Commands
{
    public static class FileManagerCommands
    {
        public static readonly RoutedUICommand OpenDriveDropdownCommand = new RoutedUICommand("Open Drive Dropdown", "OpenDriveDropdown", typeof(FileManagerWindow));
        public static readonly RoutedUICommand SettingsCommand = new RoutedUICommand("Settings...", "Settings", typeof(FileManagerWindow));
        public static readonly RoutedUICommand AboutCommand = new RoutedUICommand("About", "About", typeof(FileManagerWindow));
        public static readonly RoutedUICommand ExitCommand = new RoutedUICommand("Quit", "Quit", typeof(FileManagerWindow), new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            });
    }
}