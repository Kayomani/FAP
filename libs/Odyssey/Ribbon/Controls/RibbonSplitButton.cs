using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Markup;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls
{
    [TemplatePart(Name = partDropDown)]
    [ContentProperty("Items")]
    public class RibbonSplitButton : RibbonDropDownButton
    {
        const string partDropDown = "PART_DropDown";

        static RibbonSplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSplitButton), new FrameworkPropertyMetadata(typeof(RibbonSplitButton)));
        }


        protected  Control DropDownButton {get;private set;}


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (DropDownButton != null)
            {
                DropDownButton.MouseDown -= OnDropDownButtonDown;
                DropDownButton.MouseUp -= OnDropDownButtonUp;
            }
            DropDownButton = GetTemplateChild(partDropDown) as Control;
            if (DropDownButton != null)
            {
                DropDownButton.MouseLeftButtonDown += new MouseButtonEventHandler(OnDropDownButtonDown);
                DropDownButton.MouseLeftButtonUp += new MouseButtonEventHandler(OnDropDownButtonUp);
            }
        }

        protected virtual void OnDropDownButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            IsDropDownPressed ^= true;
            EnsurePopupRemainsOnMouseUp();
        }

        protected virtual void OnDropDownButtonUp(object sender, MouseButtonEventArgs e)
        {
            EnsurePopupDoesNotStayOpen();
        }




        public ClickMode ClickMode
        {
            get { return (ClickMode)GetValue(ClickModeProperty); }
            set { SetValue(ClickModeProperty, value); }
        }

        public static readonly DependencyProperty ClickModeProperty =
            DependencyProperty.Register("ClickMode", typeof(ClickMode), typeof(RibbonSplitButton), new UIPropertyMetadata(ClickMode.Release));


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsPressed = true;
            EnsurePopupRemainsOnMouseUp();
            if (ClickMode == ClickMode.Press) PerformClick();
        }

        protected override void HandleMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //base.HandleMouseLeftButtonDown(e);
        }

        protected override void HandleMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //base.HandleMouseLeftButtonUp(e);
        }


        private void PerformClick()
        {
            OnClick();
        }

        protected override void OnClick()
        {
            if ((Command != null) && Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
            base.OnClick();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool wasPressed = IsPressed;
            IsPressed = false;
            base.OnMouseLeftButtonUp(e);
            EnsurePopupDoesNotStayOpen();
            if (wasPressed && ClickMode == ClickMode.Release) PerformClick();

        }

        protected override void ToggleDropDownState()
        {
            // do not show the popup menu at this place.
        }

    }
}
