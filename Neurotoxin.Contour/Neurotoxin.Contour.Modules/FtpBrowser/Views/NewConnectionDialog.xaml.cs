using System;
using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class NewConnectionDialog : Window
    {
        public NewConnectionDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }
    }
}