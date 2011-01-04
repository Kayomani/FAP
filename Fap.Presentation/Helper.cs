#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.IO;

namespace Wpf.Controls
{
    class Dimension
    {
        public double Height;
        public double MaxHeight = double.PositiveInfinity;
        public double MinHeight;
        public double Width;
        public double MaxWidth = double.PositiveInfinity;
        public double MinWidth;
    }

    class Helper
    {        
        /// <summary>
        /// Find a specific parent object type in the visual tree
        /// </summary>
        public static T FindParentControl<T>(DependencyObject outerDepObj) where T : DependencyObject
        {
            DependencyObject dObj = VisualTreeHelper.GetParent(outerDepObj);
            if (dObj == null)
                return null;

            if (dObj is T)
                return dObj as T;

            while ((dObj = VisualTreeHelper.GetParent(dObj)) != null)
            {
                if (dObj is T)
                    return dObj as T;
            }
            
            return null;
        }

        /// <summary>
        /// Find the Panel for the TabControl
        /// </summary>
        public static TabPanel FindVirtualizingTabPanel(Visual visual)
        {
            if (visual == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = VisualTreeHelper.GetChild(visual, i) as Visual;

                if (child != null)
                {
                    if (child is TabPanel)
                    {
                        object temp = child;
                        return (TabPanel)temp;
                    }

                    TabPanel panel = FindVirtualizingTabPanel(child);
                    if (panel != null)
                    {
                        object temp = panel;
                        return (TabPanel)temp; // return the panel up the call stack
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Clone an element
        /// </summary>
        /// <param name="elementToClone"></param>
        /// <returns></returns>
        public static object CloneElement(object elementToClone)
        {
            string xaml = XamlWriter.Save(elementToClone);
            return XamlReader.Load(new XmlTextReader(new StringReader(xaml)));
        }

    }
}
