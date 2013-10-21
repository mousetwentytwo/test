using System;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Controls;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class NotificationMessage
    {
        private bool _isClosable;

        public NotificationMessage(string title, string message, bool isCloseable = true)
        {
            Owner = Application.Current.MainWindow;
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