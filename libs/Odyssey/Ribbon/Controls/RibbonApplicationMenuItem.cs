using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls
{
    public class RibbonApplicationMenuItem:RibbonMenuItem
    {
        static RibbonApplicationMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonApplicationMenuItem), new FrameworkPropertyMetadata(typeof(RibbonApplicationMenuItem)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RibbonMenuItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RibbonApplicationMenuItem();
        }



        /// <summary>
        /// Gets or sets the title for the sub menu popup.
        /// This is a dependency property.
        /// </summary>
        public object SubMenuTitle
        {
            get { return (object)GetValue(SubMenuTitleProperty); }
            set { SetValue(SubMenuTitleProperty, value); }
        }

        public static readonly DependencyProperty SubMenuTitleProperty =
            DependencyProperty.Register("SubMenuTitle", typeof(object), typeof(RibbonApplicationMenuItem), new UIPropertyMetadata(null));



        /// <summary>
        /// Gets or sets the content that appears in the right part of the ApplicationMenu.
        /// </summary>
        public object SubMenuContent
        {
            get { return (object)GetValue(SubMenuContentProperty); }
            set { SetValue(SubMenuContentProperty, value); }
        }

        public static readonly DependencyProperty SubMenuContentProperty =
            DependencyProperty.Register("SubMenuContent", typeof(object), typeof(RibbonApplicationMenuItem), new UIPropertyMetadata(null));



    }
}
