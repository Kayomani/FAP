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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Odyssey.Controls.Interfaces;

namespace Odyssey.Controls
{

    /// <summary>
    /// A TextBox that contains buttons on the right.
    /// </summary>
    [TemplatePart(Name = partTextBox)]
    public class OdcTextBox : TextBox,IKeyTipControl
    {
        private const string partTextBox = "PART_TextBox";

        static OdcTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OdcTextBox), new FrameworkPropertyMetadata(typeof(OdcTextBox)));
            UIElement.FocusableProperty.OverrideMetadata(typeof(OdcTextBox),new FrameworkPropertyMetadata(true));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(OdcTextBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(OdcTextBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(OdcTextBox), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }




        private ObservableCollection<ButtonBase> buttons = new ObservableCollection<ButtonBase>();

        /// <summary>
        /// Gets the collection of buttons to appear on the right of the OdcTextBox.
        /// </summary>
        public ObservableCollection<ButtonBase> Buttons
        {
            get { return buttons; }
        }


        /// <summary>
        /// Gets or sets the text that appears when the textbox is empty.
        /// This is a dependency property.
        /// </summary>
        public string Info
        {
            get { return (string)GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(string), typeof(OdcTextBox), new UIPropertyMetadata(""));



        #region IKeyTipControl Members

        void IKeyTipControl.ExecuteKeyTip()
        {
            Focus();
            SelectAll();
        }

        #endregion
    }
}
