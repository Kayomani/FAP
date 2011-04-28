using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

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
    public class ApplyPropertiesEventArgs:RoutedEventArgs
    {
        public ApplyPropertiesEventArgs(object item, BreadcrumbItem breadcrumb, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Item = item;
            Breadcrumb = breadcrumb;

        }

        /// <summary>
        /// The breadcrumb for which to apply the properites.
        /// </summary>
        public BreadcrumbItem Breadcrumb { get; private set; }

        /// <summary>
        /// The data item of the breadcrumb.
        /// </summary>
        public object Item { get; private set; }

        public ImageSource Image { get; set; }

        /// <summary>
        /// The trace that is used to show the title of a breadcrumb.
        /// </summary>
        public object Trace { get; set; }

        /// <summary>
        /// The trace that is used to build the path.
        /// This can be used to remove the trace of the root item in the path, if necassary.
        /// </summary>
        public string TraceValue { get; set; }
    }

    public delegate void ApplyPropertiesEventHandler(object sender, ApplyPropertiesEventArgs e);
}
