using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Neurotoxin.Contour.Presentation.Extensions
{
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Finds the ancestor with the specified type.
        /// </summary>
        public static T FindAncestor<T>(this FrameworkElement f) where T : DependencyObject
        {
            while (true)
            {
                var p = f.Parent ?? f.TemplatedParent;
                if (p == null)
                    return null;
                if (p is T)
                    return (T)p;
                if (!(p is FrameworkElement))
                    return null;
                f = (FrameworkElement)p;
            }
        }
    }
}
