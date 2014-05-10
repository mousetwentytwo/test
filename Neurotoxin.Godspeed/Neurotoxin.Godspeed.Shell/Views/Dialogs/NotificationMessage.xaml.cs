﻿using System;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Primitives;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    [Flags]
    public enum NotificationMessageFlags
    {
        None = 0x0,
        NonClosable = 0x1
    }

    public partial class NotificationMessage
    {
        private readonly NotificationMessageFlags _flags;

        private static NotificationMessage _instance;

        public static void ShowMessage(string title, string message, NotificationMessageFlags flags = NotificationMessageFlags.None)
        {
            if (UserSettings.IsMessageIgnored(message)) return;

            if (!UIThread.IsUIThread)
            {
                UIThread.Run(() => ShowMessage(title, message, flags));
                return;
            }

            if (_instance != null) _instance.Close();

            _instance = new NotificationMessage(title, message, flags);
            _instance.ShowDialog();
        }

        public static void CloseMessage()
        {
            if (_instance != null) _instance.Close();
            _instance = null;
        }

        private NotificationMessage(string title, string message, NotificationMessageFlags flags)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            DataContext = this;
            InitializeComponent();
            Message.Text = message;
            Title = title;
            _flags = flags;
            if (flags.HasFlag(NotificationMessageFlags.NonClosable))
            {
                CloseButtonVisibility = Visibility.Collapsed;
            } 
            else
            {
                Loaded += OnLoaded;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
            Loaded -= OnLoaded;
        }

    }
}