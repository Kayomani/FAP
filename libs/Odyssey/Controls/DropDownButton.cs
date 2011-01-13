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
using System.Windows.Markup;

namespace Odyssey.Controls
{

    [TemplatePart(Name = "PART_Popup")]
    [ContentProperty("Content")]
    public class DropDownButton : RibbonSplitButton
    {
        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        public DropDownButton()
            : base()
        {
            AddHandler(ClickableTreeViewItem.ClickEvent, new RoutedEventHandler(OnMenuItemClickedEvent));

        }

        protected override UIElement PlacementTarget
        {
            get
            {
                return DropDownButton;
            }
        }


        /// <summary>
        /// Gets or sets the content in the dropdown button area.
        /// </summary>
        public object DropDownButtonContent
        {
            get { return (object)GetValue(DropDownButtonContentProperty); }
            set { SetValue(DropDownButtonContentProperty, value); }
        }

        public static readonly DependencyProperty DropDownButtonContentProperty =
            DependencyProperty.Register("DropDownButtonContent", typeof(object), typeof(DropDownButton), new UIPropertyMetadata(null));

    }
}
