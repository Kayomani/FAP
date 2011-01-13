using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace Odyssey.Controls
{
    /// <summary>
    /// Specifies how to render Images in Ribbon Controls such as RibbonButton, RibbonSplitButton, etc.
    /// </summary>
    public class ImageRenderOptions
    {
        static ImageRenderOptions()
        {
            LargeImageScalingModeProperty.AddOwner(typeof(RibbonButton));
            LargeImageScalingModeProperty.AddOwner(typeof(TextBlock));
        }

        public static BitmapScalingMode GetLargeImageScalingMode(DependencyObject obj)
        {
            return (BitmapScalingMode)obj.GetValue(LargeImageScalingModeProperty);
        }

        public static void SetLargeImageScalingMode(DependencyObject obj, BitmapScalingMode value)
        {
            obj.SetValue(LargeImageScalingModeProperty, value);
        }

        public static readonly DependencyProperty LargeImageScalingModeProperty =
            DependencyProperty.RegisterAttached("LargeImageScalingMode", 
            typeof(BitmapScalingMode), 
            typeof(ImageRenderOptions),
            new FrameworkPropertyMetadata(BitmapScalingMode.NearestNeighbor, FrameworkPropertyMetadataOptions.Inherits));



        public static BitmapScalingMode GetSmallImageScalingMode(DependencyObject obj)
        {
            return (BitmapScalingMode)obj.GetValue(SmallImageScalingModeProperty);
        }

        public static void SetSmallImageScalingMode(DependencyObject obj, BitmapScalingMode value)
        {
            obj.SetValue(SmallImageScalingModeProperty, value);
        }

        public static readonly DependencyProperty SmallImageScalingModeProperty =
            DependencyProperty.RegisterAttached("SmallImageScalingMode", 
            typeof(BitmapScalingMode), 
            typeof(ImageRenderOptions),
            new FrameworkPropertyMetadata(BitmapScalingMode.NearestNeighbor, FrameworkPropertyMetadataOptions.Inherits));





        public static EdgeMode GetLargeEdgeMode(DependencyObject obj)
        {
            return (EdgeMode)obj.GetValue(LargeEdgeModeProperty);
        }

        public static void SetLargeEdgeMode(DependencyObject obj, EdgeMode value)
        {
            obj.SetValue(LargeEdgeModeProperty, value);
        }

        public static readonly DependencyProperty LargeEdgeModeProperty =
            DependencyProperty.RegisterAttached("LargeEdgeMode", typeof(EdgeMode), typeof(ImageRenderOptions), new UIPropertyMetadata(EdgeMode.Aliased));




        public static EdgeMode GetSmallEdgeMode(DependencyObject obj)
        {
            return (EdgeMode)obj.GetValue(SmallEdgeModeProperty);
        }

        public static void SetSmallEdgeMode(DependencyObject obj, EdgeMode value)
        {
            obj.SetValue(SmallEdgeModeProperty, value);
        }

        public static readonly DependencyProperty SmallEdgeModeProperty =
            DependencyProperty.RegisterAttached("SmallEdgeMode", typeof(EdgeMode), typeof(ImageRenderOptions), new UIPropertyMetadata(EdgeMode.Aliased));





    }
}
