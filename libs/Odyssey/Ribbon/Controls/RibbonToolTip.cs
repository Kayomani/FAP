using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Odyssey.Controls
{
    /// <summary>
    /// Enhanced tooltip with additional Properties that appears below the ribbon bar.
    /// </summary>
    public class RibbonToolTip : ToolTip
    {
        static RibbonToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonToolTip), new FrameworkPropertyMetadata(typeof(RibbonToolTip)));
        }

        public RibbonToolTip()
            : base()
        {
            Placement = System.Windows.Controls.Primitives.PlacementMode.Custom;
            CustomPopupPlacementCallback = new System.Windows.Controls.Primitives.CustomPopupPlacementCallback(PopupPlacement);
        }

        /// <summary>
        /// Ensure the position of the tooltip to be exactly below the <see cref="T:RibbonBar"/> if available, otherwise use default positioning.
        /// </summary>
        private CustomPopupPlacement[] PopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            RibbonBar ribbon = GetRibbonBar();
            double y = ActualHeight;
            double x = 0.0d;

            if (ribbon != null)
            {
                FrameworkElement owner = this.PlacementTarget as FrameworkElement;
                if (!(owner is RibbonApplicationMenu))
                {
                    if (owner.IsDescendantOf(ribbon))
                    {
                        Point p = ribbon.TranslatePoint(new Point(), owner);
                        y = ribbon.ActualHeight  + p.Y;
                    }
                    else
                    {
                        Popup popup = ribbon.Popup;
                        FrameworkElement child = popup.Child as FrameworkElement;
                        if (child != null)
                        {
                            Point p = child.TranslatePoint(new Point(), owner);
                            y = child.ActualHeight + p.Y;
                        }
                    }
                }
                else
                {
                    y = owner.ActualHeight + 4.0d;
                    x = 0d;
                }
            }

            CustomPopupPlacement placement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Vertical);
            return new CustomPopupPlacement[] { placement };
        }

        private RibbonBar GetRibbonBar()
        {
            FrameworkElement parent = this.PlacementTarget as FrameworkElement;
            while (parent != null && !(parent is RibbonBar))
            {
                parent = parent.Parent != null ? parent.Parent as FrameworkElement : parent.TemplatedParent as FrameworkElement;
            }
            return parent as RibbonBar;
        }


        /// <summary>
        /// Gets or sets the title for the tooltip.
        /// This is a dependency property.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RibbonToolTip), new UIPropertyMetadata(null));



        /// <summary>
        /// Gets or sets the footer for the tooltip.
        /// This is a dependency property.
        /// </summary>
        public string Footer
        {
            get { return (string)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }

        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(string), typeof(RibbonToolTip), new UIPropertyMetadata(null));



        /// <summary>
        /// Gets or sets the description for the tooltip.
        /// This is a dependency property.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(RibbonToolTip), new UIPropertyMetadata(null));


        /// <summary>
        /// Gets or sets the Image for the tooltip.
        /// This is a dependency property.
        /// </summary>
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(RibbonToolTip), new UIPropertyMetadata(null));


        /// <summary>
        /// Gets or sets the Image for the footer of the tooltip.
        /// This is a dependency property.
        /// </summary>
        public ImageSource FooterImage
        {
            get { return (ImageSource)GetValue(FooterImageProperty); }
            set { SetValue(FooterImageProperty, value); }
        }

        public static readonly DependencyProperty FooterImageProperty =
            DependencyProperty.Register("FooterImage", typeof(ImageSource), typeof(RibbonToolTip), new UIPropertyMetadata(null));

    }
}
