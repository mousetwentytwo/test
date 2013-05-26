using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock
{
    public static class Extensions
    {
        public static FrameworkElement GetParent<T>(this FrameworkElement thisptr)
        {
            FrameworkElement p = thisptr;
            do
            {
                p = GetParent(p);
            } while (p != null && p.GetType() != typeof(T));
            return p;
        }

        public static bool HasChild(this FrameworkElement thisptr, FrameworkElement child)
        {
            if (child == null) return false;
            FrameworkElement e = child;
            do
            {
                e = GetParent(e);
            } while (e != null && e != thisptr);
            return e != null;
        }

        public static FrameworkElement GetParent(FrameworkElement element)
        {
            return element.Parent as FrameworkElement ?? element.TemplatedParent as FrameworkElement;
        }
    }
}
