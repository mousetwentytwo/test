using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace StackPanelProportionsPOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            pp.ItemsSource = new ObservableCollection<Data>
                        {
                            new Data("a", 10),
                            new Data("a", 20),
                            new Data("a", 5),
                            new Data("a", 100),
                        };
        }
    }

    public class Data
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public Data(string text, int value)
        {
            Text = text;
            Value = value;
        }
    }
}