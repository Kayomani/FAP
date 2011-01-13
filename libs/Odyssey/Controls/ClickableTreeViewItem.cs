using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Security;

namespace Odyssey.Controls
{
    /// <summary>
    /// A TreeViewItem that adds support for Click events and Commands.
    /// </summary>
    public class ClickableTreeViewItem : TreeViewItem, ICommandSource
    {
        static ClickableTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClickableTreeViewItem), new FrameworkPropertyMetadata(typeof(ClickableTreeViewItem)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ClickableTreeViewItem();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    if (ClickMode == ClickMode.Press)
                    {
                        OnClick();
                    }
                    else
                    {
                        IsPressed = true;
                    }
                    e.Handled = true;
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    IsPressed = false;
                    if (ClickMode == ClickMode.Release)
                    {
                        OnClick();
                        e.Handled = true;
                    }
                    break;
            } base.OnKeyUp(e);
        }



        public ClickMode ClickMode
        {
            get { return (ClickMode)GetValue(ClickModeProperty); }
            set { SetValue(ClickModeProperty, value); }
        }

        public static readonly DependencyProperty ClickModeProperty =
            DependencyProperty.Register("ClickMode", typeof(ClickMode), typeof(ClickableTreeViewItem), new UIPropertyMetadata(ClickMode.Release));


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            base.Focus();
            IsPressed = true;

            if (ClickMode == ClickMode.Press)
            {
                bool flag = true;
                try
                {
                    OnClick();
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        IsPressed = false;
                        base.ReleaseMouseCapture();
                    }
                }
            }
        }

        bool IsSpaceKeyDown
        {
            get
            {
                return Keyboard.IsKeyDown(Key.Space);
            }
        }

        bool wasPressed = false;

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            wasPressed = (e.LeftButton == MouseButtonState.Pressed && IsPressed);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && wasPressed)
            {
                IsPressed = true;
            }
            else if (wasPressed)
            {
                IsPressed = false;
                wasPressed = false;
            }
            base.OnMouseEnter(e);
        }



        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            bool flag = (!IsSpaceKeyDown && IsPressed) && (this.ClickMode == ClickMode.Release);
            if (base.IsMouseCaptured && !IsSpaceKeyDown)
            {
                base.ReleaseMouseCapture();
            }
            if (flag)
            {
                OnClick();
            }
            wasPressed = false;
            IsPressed = false;
        }



        /// <summary>
        /// Gets whether the ClickableTreeViewItem is pressed.
        /// </summary>
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            private set { SetValue(IsPressedPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(ClickableTreeViewItem), new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;



        [SecurityTreatAsSafe, SecurityCritical]
        protected virtual void OnClick()
        {
            OnClickImpl(false);
        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        [SecurityCritical]
        private void OnClickImpl(bool userInitiated)
        {
            RoutedEventArgs args = new RoutedEventArgs(Button.ClickEvent, this);
            if (Command != null)
            {
                if (Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                }
            }
            RaiseEvent(args);
        }

        #region events

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("ClickEvent",
            RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(Button));
        #endregion


        #region ICommandSource Members



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ClickableTreeViewItem), new UIPropertyMetadata(null));




        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ClickableTreeViewItem), new UIPropertyMetadata(null));




        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ClickableTreeViewItem), new UIPropertyMetadata(null));



        #endregion
    }
}
