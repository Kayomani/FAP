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
using System.Diagnostics;
using Odyssey.Controls.Ribbon.Interfaces;
using System.Windows.Controls.Primitives;
using Odyssey.Controls.Interfaces;
using System.Windows.Automation.Peers;

namespace Odyssey.Controls
{
    /// <summary>
    /// Enables Quick Access keys.
    /// </summary>
    public class KeyTip : Control
    {
        static KeyTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KeyTip), new FrameworkPropertyMetadata(typeof(KeyTip)));
            EventManager.RegisterClassHandler(typeof(RibbonWindow),
                FrameworkElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnPreviewWindowKeyDown), false);

            EventManager.RegisterClassHandler(typeof(RibbonWindow),
                FrameworkElement.PreviewMouseDownEvent,
                new RoutedEventHandler(OnPreviewMouseDown), false);


        }

        static void OnPreviewMouseDown(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        static void OnPreviewWindowKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.SystemKey)
            {
                case Key.LeftAlt:
                    ToggleQuickAccessKeys(sender);
                    e.Handled = true;
                    break;

                default:
                    e.Handled = CheckQuickAccessKey(e);
                    break;

            }
            switch (e.Key)
            {
                case Key.Escape:
                    if (current != null)
                    {
                        MoveBack();
                        e.Handled = true;
                        //FrameworkElement x = FocusManager.GetFocusedElement(sender as DependencyObject) as FrameworkElement;
                        //x.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    }
                    break;
            }
            if (!e.Handled)
            {
                Reset();
            }
        }

        private static void MoveBack()
        {
            selectStack.Pop();
            current = selectStack.Count > 0 ? selectStack.Pop() : null;
            elements = null;
            keySequence = "";
            Keyboard.Focus(null);
            RibbonDropDownButton.CloseOpenedPopup(null);
            RibbonGroup.CloseOpenedPopup();
            CloseQuickAccessKeys();
            if (current != null) ShowQuickAccessKeys(current);
        }

        private static void ToggleQuickAccessKeys(object sender)
        {
            Window window = (Window)sender;
            window.Deactivated -= window_LocationChanged;
            window.Deactivated += window_LocationChanged;
            window.LocationChanged -= window_LocationChanged;
            window.LocationChanged += window_LocationChanged;
            if (current == null)
            {
                ShowQuickAccessKeys(window);
            }
            else
            {
                Reset();
            }
        }


        static void window_LocationChanged(object sender, EventArgs e)
        {
            Reset();
        }

        private static void Reset()
        {
            if (elements != null)
            {
                RibbonDropDownButton.CloseOpenedPopup(null);
                RibbonGroup.CloseOpenedPopup();
                HideQuickAccessKeys();
            }
        }

        private static void HideQuickAccessKeys()
        {
            CloseQuickAccessKeys();
            elements = null;
            keySequence = "";
            current = null;
            selectStack.Clear();
        }

        private static bool CheckQuickAccessKey(KeyEventArgs e)
        {
            if (elements == null || elements.Count == 0) return false;
            string c = new KeyConverter().ConvertToInvariantString(e.Key);
            keySequence += c;

            FrameworkElement match = null;
            List<FrameworkElement> filtered = new List<FrameworkElement>();
            foreach (FrameworkElement child in elements)
            {
                string key = KeyTip.GetKey(child);
                if (keySequence.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    match = child;
                    break;
                }
                if (key.StartsWith(keySequence, StringComparison.InvariantCultureIgnoreCase))
                {
                    filtered.Add(child);
                }
            }
            if (match != null)
            {
                keySequence = null;
                //Remove the key tips befor executing, since another window might be opened and thus the key tips would be desturbing:
                HideQuickAccessKeys();
                ExecuteElement(match);
                if (KeyTip.GetStop(match))
                {
                    // ensure the matched element to be measured:
                    match.UpdateLayout();

                    ShowQuickAccessKeys(match);
                }
                else
                {
                    HideQuickAccessKeys();
                }
                return true;

            }
            if (filtered.Count > 0)
            {
                elements = filtered;
                ShowKeys(elements);
                return true;
            }
            else return false;
        }

        private static void ExecuteElement(FrameworkElement e)
        {
            IKeyTipControl cmd = e as IKeyTipControl;
            if (cmd != null)
            {
                cmd.ExecuteKeyTip();
                return;
            }
            CheckBox cb = e as CheckBox;
            if (cb != null)
            {
                cb.IsChecked ^= true;
                return;
            }
            ComboBox box = e as ComboBox;
            if (box != null)
            {
                box.Focus();
                box.IsDropDownOpen = true;
                return;
            }
            if (e.Focusable) e.Focus();
        }

        private static void ShowKeys(List<FrameworkElement> elements)
        {
            CloseQuickAccessKeys();
            popups = new List<Popup>();
            foreach (var e in elements)
            {
                double yOffset = KeyTip.GetYOffset(e);
                double xOffset = KeyTip.GetXOffset(e);
                if (xOffset < 0.0) xOffset = e.ActualWidth + xOffset;
                if (yOffset < 0.0) yOffset = e.ActualHeight + yOffset;

                if (double.IsNaN(yOffset)) yOffset = e.ActualHeight - 16;
                if (double.IsNaN(xOffset)) xOffset = 12;
                string key = KeyTip.GetKey(e);
                Popup popup = new Popup();
                popup.AllowsTransparency = true;
                popup.Child = new KeyTip() { Text = key };
                popup.PlacementTarget = e;
                popup.Placement = PlacementMode.Relative;
                popup.HorizontalOffset = xOffset;
                popup.VerticalOffset = yOffset;
                popup.StaysOpen = true;
                popups.Add(popup);
                popup.IsOpen = true;

            }
        }

        private static void CloseQuickAccessKeys()
        {
            if (popups != null)
            {
                foreach (var popup in popups)
                {
                    popup.IsOpen = false;
                }
                popups = null;
            }
        }

        private static Stack<FrameworkElement> selectStack = new Stack<FrameworkElement>();
        private static FrameworkElement current;
        private static List<Popup> popups;
        private static List<FrameworkElement> elements;
        private static string keySequence = "";

        private static void ShowQuickAccessKeys(FrameworkElement root)
        {
            selectStack.Push(root);
            current = root;
            elements = new List<FrameworkElement>();
            GatherChildElements(elements, root);

            if (elements.Count == 0)
            {
                HideQuickAccessKeys();
            }
            else
            {
                ShowKeys(elements);
            }
        }

        private static void GatherChildElements(List<FrameworkElement> elements, FrameworkElement root)
        {
            foreach (var o in LogicalTreeHelper.GetChildren(root))
            {
                FrameworkElement e = o as FrameworkElement;
                if (e != null)
                {
                    GatherElements(elements, e);
                }
            }
        }

        /// <summary>
        /// Don't call this directly, it is only used in GatherChildElements.
        /// </summary>
        private static void GatherElements(List<FrameworkElement> elements, FrameworkElement root)
        {
            if (root.Visibility != Visibility.Visible || root.IsEnabled == false) return;
            string key = KeyTip.GetKey(root);
            if (key != null)
            {
                elements.Add(root);
            }
            if (key == null || !KeyTip.GetStop(root))
            {
                foreach (var o in LogicalTreeHelper.GetChildren(root))
                {
                    FrameworkElement e = o as FrameworkElement;
                    if (e != null)
                    {
                        GatherElements(elements, e);
                    }
                }
            }
        }



        /// <summary>
        /// Gets the Quick Access Key combination.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetKey(DependencyObject obj)
        {
            return (string)obj.GetValue(KeyProperty);
        }

        /// <summary>
        /// Sets the Quick Access Key.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetKey(DependencyObject obj, string value)
        {
            obj.SetValue(KeyProperty, value);
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached("Key", typeof(string), typeof(KeyTip), new UIPropertyMetadata(null));




        public static double GetXOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(XOffsetProperty);
        }

        public static void SetXOffset(DependencyObject obj, double value)
        {
            obj.SetValue(XOffsetProperty, value);
        }

        public static readonly DependencyProperty XOffsetProperty =
            DependencyProperty.RegisterAttached("XOffset", typeof(double), typeof(KeyTip), new UIPropertyMetadata(double.NaN));




        public static double GetYOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(YOffsetProperty);
        }

        public static void SetYOffset(DependencyObject obj, double value)
        {
            obj.SetValue(YOffsetProperty, value);
        }

        // Using a DependencyProperty as the backing store for YOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YOffsetProperty =
            DependencyProperty.RegisterAttached("YOffset", typeof(double), typeof(KeyTip), new UIPropertyMetadata(double.NaN));




        /// <summary>
        /// Gets whether to stop gathering for KeyTips of child controls.
        /// </summary>
        public static bool GetStop(DependencyObject obj)
        {
            return (bool)obj.GetValue(StopProperty);
        }

        /// <summary>
        /// Sets whether to stop gathering for KeyTips of child controls. The default value is true.
        /// </summary>
        public static void SetStop(DependencyObject obj, bool value)
        {
            obj.SetValue(StopProperty, value);
        }

        // Using a DependencyProperty as the backing store for Stop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopProperty =
            DependencyProperty.RegisterAttached("Stop", typeof(bool), typeof(KeyTip), new UIPropertyMetadata(true));





        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(KeyTip), new UIPropertyMetadata(null));





    }
}
