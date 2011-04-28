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
using System.Windows.Markup;
using System.Diagnostics;
using Odyssey.Controls.Ribbon.Interfaces;
using System.ComponentModel;
using Odyssey.Common;
using Odyssey.Controls.Interfaces;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls
{

    [TemplatePart(Name = partPopup)]
    [ContentProperty("Items")]
    public class RibbonDropDownButton : ItemsControl, IRibbonButton, ICommandSource,IKeyTipControl
    {
        const string partPopup = "PART_Popup";

        static RibbonDropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonDropDownButton), new FrameworkPropertyMetadata(typeof(RibbonDropDownButton)));
        }

        public RibbonDropDownButton()
        {
            AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnMenuItemClickedEvent));
            AddHandler(RibbonButton.ClickEvent, new RoutedEventHandler(OnButtonClickEvent));
            AddHandler(RibbonComboBox.DropDownClosedEvent, new RoutedEventHandler(OnMenuItemClickedEvent));
            AddHandler(RibbonDropDownButton.PopupClosedEvent, new RoutedEventHandler(OnPopupClosedEvent));
            AddHandler(RibbonGallery.SelectionChangedEvent, new RoutedEventHandler(OnGalerySelected));
           
        }

        protected virtual void OnGalerySelected(object sender, RoutedEventArgs e)
        {
            IsDropDownPressed = false;
        }

        /// <summary>
        /// If any RibbonDropDownButton has closed it's popup, so also close it.
        /// This is necassary for nested RibbonDropDownButtons to ensure that all are properly closed.
        /// </summary>
        protected virtual void OnPopupClosedEvent(object sender, RoutedEventArgs e)
        {
            if (IsDropDownPressed)
            {
                IsDropDownPressed = false;
            }
        }

        protected virtual void OnButtonClickEvent(object sender, RoutedEventArgs e)
        {
            if (IsDropDownPressed)
            {
                if (e.OriginalSource == this) return;
                DependencyObject dep = e.OriginalSource as DependencyObject;
                if (!(e.OriginalSource is RibbonButton) && !(e.OriginalSource is RibbonDropDownButton))
                {
                    if (dep != null && !RibbonOption.GetCloseDropDownOnClick(dep)) return;
                }
                else
                {
                    if (dep != null && !RibbonBar.GetAffectsDropDown(dep)) return;
                }
                if (IsAncestorType(e.OriginalSource, typeof(RibbonComboBox))) return;
                IsDropDownPressed = false;
            }
        }

        protected virtual void OnMenuItemClickedEvent(object sender, RoutedEventArgs e)
        {
            if (IsDropDownPressed)
            {
                if (e.OriginalSource == this) return;
                IsDropDownPressed = false;
            }
        }

        private bool IsAncestorType(object child, Type type)
        {
            FrameworkElement parent = child as FrameworkElement;
            while (parent != null)
            {
                if (parent.TemplatedParent != null && parent.TemplatedParent.GetType() == type) return true;
                if (parent.GetType() == type) return true;
                parent = parent.Parent as FrameworkElement;
            }
            return false;
        }


        internal protected Popup Popup { get; private set; }




        public override void OnApplyTemplate()
        {
            if (Popup != null)
            {
                Popup.Closed -= OnPopupClosed;
                Popup.Opened -= OnPopupOpened;
            }
            Popup = GetTemplateChild(partPopup) as Popup;
            if (Popup != null)
            {
                Popup.Closed += new EventHandler(OnPopupClosed);
                Popup.Opened += new EventHandler(OnPopupOpened);
                Popup.StaysOpen = true;
            }

            //   base.OnApplyTemplate();
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
    RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RibbonDropDownButton));




        public bool IsCheckable
        {
            get { return (bool)GetValue(IsCheckableProperty); }
            set { SetValue(IsCheckableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCheckable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckableProperty =
            DependencyProperty.Register("IsCheckable", typeof(bool), typeof(RibbonDropDownButton), new UIPropertyMetadata(false));



        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(RibbonDropDownButton), new UIPropertyMetadata(false, CheckedPropertyChanged));



        private static void CheckedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            protected set { SetValue(IsPressedPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(RibbonDropDownButton), new UIPropertyMetadata(false));

        public static DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;


        protected virtual void OnClick()
        {
            ToggleChecked();
        }

        private void ToggleChecked()
        {
            if (IsCheckable)
            {
                Rect rect = new Rect(new Point(), base.RenderSize);
                if (((Mouse.LeftButton == MouseButtonState.Pressed) && base.IsMouseOver) && rect.Contains(Mouse.GetPosition(this)))
                {

                    if (IsChecked) base.ClearValue(IsCheckedProperty); else IsChecked = true;
                }
            }
        }


        public static readonly RoutedEvent PopupOpenedEvent = EventManager.RegisterRoutedEvent("PopupOpened",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RibbonDropDownButton));

        public static readonly RoutedEvent PopupClosedEvent = EventManager.RegisterRoutedEvent("PopupClosed",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RibbonDropDownButton));

        public event RoutedEventHandler PopupOpened
        {
            add { AddHandler(PopupOpenedEvent, value); }
            remove { RemoveHandler(PopupOpenedEvent, value); }
        }

        public event RoutedEventHandler PopupClosed
        {
            add { AddHandler(PopupClosedEvent, value); }
            remove { RemoveHandler(PopupClosedEvent, value); }
        }

        protected virtual void OnPopupOpened(object sender, EventArgs e)
        {

            IsDropDownPressed = true;
            RoutedEventArgs args = new RoutedEventArgs(RibbonDropDownButton.PopupOpenedEvent);
            if (Popup != null && Popup.Child != null) Popup.Child.Focus();
            RaiseEvent(args);
            // Mouse.Capture(this, CaptureMode.SubTree);
        }

        protected virtual void OnPopupClosed(object sender, EventArgs e)
        {
            if (Mouse.Captured == this)
            {
                Mouse.Capture(null);
            }

            IsChecked = false;
            //base.ClearValue(IsDropDownPressedProperty);
            isDropDownOpen = false;
            RoutedEventArgs args = new RoutedEventArgs(RibbonDropDownButton.PopupClosedEvent);
            RaiseEvent(args);
        }



        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));




        public ImageSource SmallImage
        {
            get { return (ImageSource)GetValue(SmallImageProperty); }
            set { SetValue(SmallImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallImageProperty =
            DependencyProperty.Register("SmallImage", typeof(ImageSource), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));



        public ImageSource LargeImage
        {
            get { return (ImageSource)GetValue(LargeImageProperty); }
            set { SetValue(LargeImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeImageProperty =
            DependencyProperty.Register("LargeImage", typeof(ImageSource), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(RibbonDropDownButton), new UIPropertyMetadata(new CornerRadius(3)));


        /// <summary>
        /// Gets or sets whether the drop down button is down.
        /// </summary>
        public bool IsDropDownPressed
        {
            get { return (bool)GetValue(IsDropDownPressedProperty); }
            set
            {
                isDropDownOpen = value;
                SetValue(IsDropDownPressedProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsDropDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropDownPressedProperty =
            DependencyProperty.Register("IsDropDownPressed", typeof(bool), typeof(RibbonDropDownButton),
            new UIPropertyMetadata(false, IsDropDownPressedPropertyChangedCallback));

        static void IsDropDownPressedPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newValue = (bool)e.NewValue;
            RibbonDropDownButton btn = d as RibbonDropDownButton;

            if (newValue) Mouse.Capture(btn, CaptureMode.SubTree);
            btn.OnDropDownPressedChanged((bool)e.OldValue, (bool)e.NewValue);

        }


        /// <summary>
        /// Gets the target for the popup placement.
        /// </summary>
        protected virtual UIElement PlacementTarget
        {
            get { return this; }
        }

        protected virtual void OnDropDownPressedChanged(bool oldValue, bool newValue)
        {
            UpdateToolTip(newValue);

            if (Popup != null)
            {
                if (newValue)
                {
                    Popup.PlacementTarget = PlacementTarget;
                    CloseOpenedPopup(this);
                }
                else
                {
                    CloseOpenedPopup(null);
                }
                Popup.IsOpen = newValue;
            }
        }

        object toolTip;

        /// <summary>
        /// When the popup is open, the tooltip must not be shown.
        /// </summary>
        private void UpdateToolTip(bool newValue)
        {
            if (newValue)
            {
                toolTip = ToolTip;
                ToolTip = null;
            }
            else
            {
                ToolTip = toolTip;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space) { IsDropDownPressed = true; e.Handled = true; }
            switch (e.Key)
            {
                case Key.Escape:
                    IsDropDownPressed = false;
                    e.Handled = true;
                    break;

                //case Key.Down:
                //    SelectNext();
                //    e.Handled = true;
                //    break;
            }
            base.OnKeyDown(e);
        }

        private void SelectNext()
        {
            ItemsPresenter ip = GetTemplateChild("PART_Items") as ItemsPresenter;
            if (ip != null)
            {
                ip.Focus();
            }
        }

        private bool isDropDownOpen = false;

        private static RibbonDropDownButton DroppedDownButton;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            HandleMouseLeftButtonDown(e);
            base.OnMouseLeftButtonDown(e);
        }

        protected virtual void HandleMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                UpdateIsPressed();
                if (!IsDropDownPressed)
                {
                    EnsurePopupRemainsOnMouseUp();
                    if (this.IsAncestorOf(e.OriginalSource as DependencyObject))
                    {
                        ToggleDropDownState();
                        e.Handled = true;
                    }
                }
                else
                {
                    ToggleDropDownState();
                }
                OnClick();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.UpdateIsPressed();
        }

        /// <summary>
        /// Code snipped from original MenuItem class using Reflector:
        /// </summary>
        private void UpdateIsPressed()
        {
            Rect rect = new Rect(new Point(), base.RenderSize);
            if (((Mouse.LeftButton == MouseButtonState.Pressed) && base.IsMouseOver) && rect.Contains(Mouse.GetPosition(this)))
            {
                this.IsPressed = true;
            }
            else
            {
                base.ClearValue(IsPressedPropertyKey);
            }

        }

        protected override void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusedChanged(e);
            if (this.IsDropDownPressed && !base.IsKeyboardFocusWithin)
            {
                DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;
                if ((focusedElement == null) || (!this.IsDropDownPressed && (ItemsControl.ItemsControlFromItemContainer(focusedElement) != this)))
                {
                    IsDropDownPressed = false;
                }
            }
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsDropDownPressed)
            {
                UIElement originalSource = e.OriginalSource as UIElement;

                if (!(this.IsLogicalAncestorOf(originalSource)))
                {
                    IsDropDownPressed = false;
                    e.Handled = true;
                }
            }
            base.OnPreviewMouseLeftButtonDown(e);

        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            if (IsDropDownPressed)
            {
                FrameworkElement fe = e.OriginalSource as FrameworkElement;
                FrameworkElement captured = Mouse.Captured as FrameworkElement;
                if (captured != this && Popup != null)
                {
                    UIElement child = this.Popup.Child;
                    if (e.OriginalSource == this)
                    {
                        if ((Mouse.Captured == null) || !child.IsLogicalAncestorOf(Mouse.Captured as UIElement))
                        {
                            this.IsDropDownPressed = false;
                            e.Handled = true;
                        }
                    }
                    else if (child.IsAncestorOf(e.OriginalSource as DependencyObject))
                    {
                        if (this.IsDropDownPressed && (Mouse.Captured == null))
                        {
                            Mouse.Capture(this, CaptureMode.SubTree);
                            e.Handled = true;
                        }
                    }
                    else if (!this.IsLogicalAncestorOf(Mouse.Captured as UIElement))
                    {
                        IsDropDownPressed = false;
                    }
                }
                //       if (IsDropDownPressed) Mouse.Capture(this, CaptureMode.SubTree);
            }
        }




        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            HandleMouseLeftButtonUp(e);
            base.OnMouseLeftButtonUp(e);
        }

        protected virtual void HandleMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                UpdateIsPressed();
                isDropDownOpen = IsDropDownPressed;
            }
        }

        protected virtual void ToggleDropDownState()
        {
            Rect rect = new Rect(new Point(), base.RenderSize);
            if (((Mouse.LeftButton == MouseButtonState.Pressed) && base.IsMouseOver) && rect.Contains(Mouse.GetPosition(this)))
            {
                isDropDownOpen ^= true;
                SetValue(IsDropDownPressedProperty, isDropDownOpen);
            }
        }


        public static void CloseOpenedPopup(RibbonDropDownButton caller)
        {
            RibbonDropDownButton btn = DroppedDownButton;
            if (btn != null && (btn != caller))
            {
                FrameworkElement parent = btn.Popup != null ? btn.Popup.Child as FrameworkElement : btn;
                if (!parent.IsLogicalAncestorOf(caller))
                {
                    if (btn.Popup != null) btn.Popup.IsOpen = false;
                    btn.IsDropDownPressed = false;
                }
            }
            DroppedDownButton = caller;
        }


        public bool IsFlat
        {
            get { return (bool)GetValue(IsFlatProperty); }
            set { SetValue(IsFlatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlatProperty =
            DependencyProperty.Register("IsFlat", typeof(bool), typeof(RibbonDropDownButton), new UIPropertyMetadata(true));


        public bool ShowDropDownSymbol
        {
            get { return (bool)GetValue(ShowDropDownSymbolProperty); }
            set { SetValue(ShowDropDownSymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowDropDownSymbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowDropDownSymbolProperty =
            DependencyProperty.Register("ShowDropDownSymbol", typeof(bool), typeof(RibbonDropDownButton), new UIPropertyMetadata(true));



        /// <summary>
        /// Gets or sets the maximum height for the dropdown panel.
        /// This is a dependency property.
        /// </summary>
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxDropDownHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(RibbonDropDownButton), new UIPropertyMetadata(double.NaN));


        /// <summary>
        /// An item can be any possible element, not necassarily (but prefered) a RibbonMenuItem:
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuItem || item is Separator;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RibbonMenuItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (item is Separator)
            {
                Control c = element as Control;
                if (c != null)
                {
                    c.IsEnabled = false;
                    c.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                }
            }
        }


        public object DropDownFooter
        {
            get { return (object)GetValue(DropDownFooterProperty); }
            set { SetValue(DropDownFooterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownFooter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownFooterProperty =
            DependencyProperty.Register("DropDownFooter", typeof(object), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));




        public object DropDownHeader
        {
            get { return (object)GetValue(DropDownHeaderProperty); }
            set { SetValue(DropDownHeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownHeader.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownHeaderProperty =
            DependencyProperty.Register("DropDownHeader", typeof(object), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));



        public DataTemplate DropDownHeaderTemplate
        {
            get { return (DataTemplate)GetValue(DropDownHeaderTemplateProperty); }
            set { SetValue(DropDownHeaderTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownHeaderTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownHeaderTemplateProperty =
            DependencyProperty.Register("DropDownHeaderTemplate", typeof(DataTemplate), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));



        public DataTemplate DropDownFooterTemplate
        {
            get { return (DataTemplate)GetValue(DropDownFooterTemplateProperty); }
            set { SetValue(DropDownFooterTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownFooterTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownFooterTemplateProperty =
            DependencyProperty.Register("DropDownFooterTemplate", typeof(DataTemplate), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));

        protected void EnsurePopupRemainsOnMouseUp()
        {
            if (Popup != null) Popup.StaysOpen = true;
        }

        protected void EnsurePopupDoesNotStayOpen()
        {
            //if (popup != null)
            //{
            //    popup.StaysOpen = false;
            //}
        }



        public PopupAnimation PopupAnimation
        {
            get { return (PopupAnimation)GetValue(PopupAnimationProperty); }
            set { SetValue(PopupAnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupAnimation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupAnimationProperty =
            DependencyProperty.Register("PopupAnimation", typeof(PopupAnimation), typeof(RibbonDropDownButton), new UIPropertyMetadata(PopupAnimation.Fade));




        public PlacementMode PopupPlacement
        {
            get { return (PlacementMode)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupPlacement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupPlacementProperty =
            DependencyProperty.Register("PopupPlacement", typeof(PlacementMode), typeof(RibbonDropDownButton), new UIPropertyMetadata(PlacementMode.Bottom));




        /// <summary>
        /// Gets or sets the with for the drop down menu.
        /// This is a dependency property.
        /// </summary>
        public double DropDownWidth
        {
            get { return (double)GetValue(DropDownWidthProperty); }
            set { SetValue(DropDownWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropDownWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropDownWidthProperty =
            DependencyProperty.Register("DropDownWidth", typeof(double), typeof(RibbonDropDownButton), new UIPropertyMetadata(double.NaN));



        public BitmapScalingMode BitmapScalingMode
        {
            get { return (BitmapScalingMode)GetValue(BitmapScalingModeProperty); }
            set { SetValue(BitmapScalingModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapScalingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitmapScalingModeProperty =
            DependencyProperty.Register("BitmapScalingMode", typeof(BitmapScalingMode), typeof(RibbonDropDownButton), new UIPropertyMetadata(BitmapScalingMode.NearestNeighbor));




        public EdgeMode EdgeMode
        {
            get { return (EdgeMode)GetValue(EdgeModeProperty); }
            set { SetValue(EdgeModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EdgeMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EdgeModeProperty =
            DependencyProperty.Register("EdgeMode", typeof(EdgeMode), typeof(RibbonDropDownButton), new UIPropertyMetadata(EdgeMode.Aliased));


        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                List<object> list = new List<object>();
                if (DropDownHeader != null) list.Add(DropDownHeader);
                foreach (var item in Items)
                {
                    list.Add(item);
                }
                if (DropDownFooter != null) list.Add(DropDownFooter);
                return list.GetEnumerator();
            }
        }

        #region ICommandSource Members



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(RibbonDropDownButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCommandPropertyChanged)));


        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RibbonDropDownButton btn = (RibbonDropDownButton)d;
            btn.OnCommandChanged(btn, (ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        // Keep a copy of the handler so it doesn't get garbage collected.
        private EventHandler canExecuteChangedHandler;

        protected virtual void OnCommandChanged(object sender, ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null) oldCommand.CanExecuteChanged -= OnCanExecuteChanged;
            if (newCommand != null)
            {
                canExecuteChangedHandler = new EventHandler(OnCanExecuteChanged);
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
            }

            UpdateCanExecute();
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            this.UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            if (this.Command != null)
            {
                this.CanExecute = CanExecuteCommandSource(this);              
            }
            else
            {
                this.CanExecute = true;
            }

            // finally notify the ui to reflect the changes:
            CoerceValue(IsEnabledProperty);
        }

        private bool CanExecuteCommandSource(ICommandSource commandSource)
        {
            ICommand command = commandSource.Command;
            if (command == null)
            {
                return false;
            }
            object commandParameter = commandSource.CommandParameter;
            IInputElement commandTarget = commandSource.CommandTarget;
            RoutedCommand command2 = command as RoutedCommand;
            if (command2 == null)
            {
                return command.CanExecute(commandParameter);
            }
            if (commandTarget == null)
            {
                commandTarget = commandSource as IInputElement;
            }
            return command2.CanExecute(commandParameter, commandTarget);
        }


        private bool CanExecute = true;



        protected override bool IsEnabledCore
        {
            get
            {
                return base.IsEnabledCore && CanExecute;
            }
        }


        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));


        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(RibbonDropDownButton), new UIPropertyMetadata(null));



        #endregion

        #region IKeyboardCommand Members

        public void ExecuteKeyTip()
        {
            Focus();
            IsDropDownPressed = true;
        }

        #endregion


    }
}

