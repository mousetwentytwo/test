using System;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Controls;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NotificationMessage
    {
        private readonly bool _isClosable;

        public static void Show(string title, string message, bool isCloseable = true)
        {
            var instance = new NotificationMessage(title, message, isCloseable);
            instance.ShowDialog();
        }

        public NotificationMessage(string title, string message, bool isCloseable = true)
        {
            if (Application.Current.MainWindow.IsVisible) Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = message;
            Title = title;
            _isClosable = isCloseable;
            if (isCloseable)
            {
                Loaded += OnLoaded;
            } 
            else
            {
                Ok.Visibility = Visibility.Collapsed;
            }
        }

        public override void OnApplyTemplate()
        {
            if (_isClosable) return;
            var buttons = Template.FindName("CaptionButtons", this) as CaptionButtons;
            buttons.Visibility = Visibility.Collapsed;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Ok.Focus();
        }

    }
}