using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Neurotoxin.Godspeed.Presentation.Controls;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Neurotoxin.Godspeed.Presentation.Extensions;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            SetVersion();
            Loaded += OnLoaded;
        }

        private void SetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            Version.Text = "v" + assemblyName.Version;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Ok.Focus();
            this.FindDescendants<RichTextBox>().ForEach(b =>
                                                            {
                                                                b.IsHitTestVisible = true;
                                                                b.SelectionOpacity = 0;
                                                            });

            var links = this.FindDescendants<RichTextBox>().SelectMany(b => b.Document.GetElementsOfType<Hyperlink>()).ToList();
            links.ForEach(l =>
                              {
                                  l.PreviewMouseDown += LOnPreviewMouseDown;
                                  l.MouseDown += LOnMouseDown;
                                  l.MouseUp += LOnMouseUp;
                                  l.PreviewMouseUp += LOnPreviewMouseUp;
                                  l.Click += LOnClick;
                                  l.RequestNavigate += OnHyperlinkRequestNavigate;
                              });
        }

        private void LOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            

        }

        private void LOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            

        }

        private void LOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            

        }

        private void LOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            

        }

        private void LOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            

        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}