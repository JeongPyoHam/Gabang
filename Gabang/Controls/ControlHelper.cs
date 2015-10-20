using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gabang.Controls
{
    public class ControlHelper
    {
        public static DependencyObject GetChild(DependencyObject reference, Type type)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);
                if (child.GetType() == type)
                {
                    return child;
                }

                var found = GetChild(child, type);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public static DependencyObject GetChild(DependencyObject reference, string name)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(reference);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(reference, i);
                var element = child as FrameworkElement;
                if (element != null)
                {
                    if (element.Name == name) return child;
                }

                var found = GetChild(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as FrameworkElement;
            }

            return null;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }
    }
}
