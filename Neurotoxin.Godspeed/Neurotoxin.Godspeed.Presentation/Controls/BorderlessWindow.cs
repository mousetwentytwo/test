using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    public class BorderlessWindow : Window
    {
        static BorderlessWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderlessWindow), new FrameworkPropertyMetadata(typeof(BorderlessWindow)));
        }
    }
}
