using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Odyssey.Controls
{
    public class RibbonOption
    {

        public static bool GetCloseDropDownOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseDropDownOnClickProperty);
        }

        public static void SetCloseDropDownOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseDropDownOnClickProperty, value);
        }

        public static readonly DependencyProperty CloseDropDownOnClickProperty =
            DependencyProperty.RegisterAttached("CloseDropDownOnClick", typeof(bool), typeof(RibbonOption),
                     new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.Inherits));



        /// <summary>
        /// Gets whether a RibbonButton, RibbonSplitButton, RibbonToggleButton or RibbonDropDownButton should transite it state with animation.
        /// This is an attached inheritable dependency property. The default value is false.
        /// </summary>
        public static bool GetAnimateTransition(DependencyObject obj)
        {
            return (bool)obj.GetValue(AnimateTransitionProperty);
        }

        /// <summary>
        /// Sets whether a RibbonButton, RibbonSplitButton, RibbonToggleButton or RibbonDropDownButton should transite it state with animation.
        /// This is an attached inheritable dependency property. The default value is false.
        /// </summary>
        public static void SetAnimateTransition(DependencyObject obj, bool value)
        {
            obj.SetValue(AnimateTransitionProperty, value);
        }

        public static readonly DependencyProperty AnimateTransitionProperty =
            DependencyProperty.RegisterAttached("AnimateTransition", typeof(bool), typeof(RibbonOption), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));



    }
}
