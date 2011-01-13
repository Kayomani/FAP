using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#region Licence
// Copyright (c) 2008 Thomas Gerber
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
namespace Odyssey.Controls
{
    /// <summary>
    /// RoutedEventArgs to convert the display path to edit path and vice verca.
    /// </summary>
    public class PathConversionEventArgs:RoutedEventArgs
    {
        //TODO: Rename PathConverionMode to LogicalToVisual and VisualToLogical
        /// <summary>
        /// Specifies what property to convert.
        /// </summary>
        public enum ConversionMode
        {
            /// <summary>
            /// Convert the display path to edit path.
            /// </summary>
            DisplayToEdit,

            /// <summary>
            /// convert the edit path to display path.
            /// </summary>
            EditToDisplay,
        }

        /// <summary>
        /// Gets or sets the display path.
        /// </summary>
        public string DisplayPath { get; set; }

        /// <summary>
        /// Gets or sets the edit path.
        /// </summary>
        public string EditPath { get; set; }

        /// <summary>
        /// Specifies what path property to convert.
        /// </summary>
        public ConversionMode Mode { get; private set; }

        /// <summary>
        /// Gets the root object of the breadcrumb bar.
        /// </summary>
        public object Root { get; private set; }

        /// <summary>
        /// Creates a new PathConversionEventArgs class.
        /// </summary>
        /// <param name="mode">The conversion mode.</param>
        /// <param name="path">The initial values for DisplayPath and EditPath.</param>
        /// <param name="root">The root object.</param>
        /// <param name="routedEvent"></param>
        public PathConversionEventArgs(ConversionMode mode, string path, object root, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Mode = mode;
            DisplayPath = EditPath = path;
            Root = root;
        }
    }

    public delegate void PathConversionEventHandler(object sender, PathConversionEventArgs e);

}
