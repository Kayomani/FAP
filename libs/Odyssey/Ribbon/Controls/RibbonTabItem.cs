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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Odyssey.Controls.Classes;
using Odyssey.Controls.Ribbon.Interfaces;
using Odyssey.Controls.Interfaces;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls
{

    [ContentProperty("Items")]
    public class RibbonTabItem : ItemsControl, IKeyTipControl
    {

        static RibbonTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTabItem), new FrameworkPropertyMetadata(typeof(RibbonTabItem)));

            // changing the visibility requires the RibbonBar to be rendered again, so ensure the correct metadata options to be set:
            VisibilityProperty.OverrideMetadata(
                typeof(RibbonTabItem),
                new FrameworkPropertyMetadata(Visibility.Visible,
                    FrameworkPropertyMetadataOptions.AffectsParentArrange
                    | FrameworkPropertyMetadataOptions.AffectsParentArrange
                    | FrameworkPropertyMetadataOptions.AffectsParentMeasure
                    | FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public RibbonTabItem()
            : base()
        {
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(OnVisibleChanged);
        }

        /// <summary>
        /// CAUTION:
        /// Call ApplyTemplate after EndInit to ensure that all controls properties could have applied to a Binding.
        /// If ApplyTemplate is not executed here, bindings like "Binding IsChecked, ElementName=anyname" would not work if this group is
        /// not yet visibile, for instance, if it is in a RibbonTab that is not the initial first tab.
        /// </summary>
        public override void EndInit()
        {
            base.EndInit();
            ApplyTemplate();
        }

        protected virtual void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;
            if (!visible) this.RibbonBar.EnsureTabIsVisible();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RibbonGroup;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RibbonGroup();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        static Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        private ObservableCollection<RibbonGroup> groups = new ObservableCollection<RibbonGroup>();

        [Obsolete("for backward compatibility only. Use Items instead!")]
        public ItemCollection Groups { get { return Items; } }

        /// <summary>
        /// Gets or sets the title for the tab item.
        /// This is a dependency property.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RibbonTabItem), new UIPropertyMetadata(""));


        /// <summary>
        /// Gets whether the tab item is a contextual tab item.
        /// This is a dependency property.
        /// </summary>
        public bool IsContextual
        {
            get { return (bool)GetValue(IsContextualProperty); }
            internal set { SetValue(IsContextualPropertyKey, value); }
        }

        internal RibbonContextualTabSet tabSet;

        // Using a DependencyProperty as the backing store for IsContextual.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey IsContextualPropertyKey =
            DependencyProperty.RegisterReadOnly("IsContextual", typeof(bool), typeof(RibbonTabItem), new UIPropertyMetadata(false));

        public static DependencyProperty IsContextualProperty = IsContextualPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets whether the tab item is selected.
        /// This is a dependency propert.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            internal set { SetValue(IsSelectedPropertyKey, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey IsSelectedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsSelected", typeof(bool), typeof(RibbonTabItem), new UIPropertyMetadata(false, null, CoerceIsSelected));

        public static object CoerceIsSelected(DependencyObject d, object baseValue)
        {
            RibbonTabItem item = (RibbonTabItem)d;
            if (item.RibbonBar.CanMinimize && !item.RibbonBar.IsExpanded)
            {
                return false;
            }
            return baseValue;
        }

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (RibbonBar != null) RibbonBar.SelectedTabItem = this;
            e.Handled = true;
        }

        private RibbonBar ribbon;

        /// <summary>
        /// Gets the RibbonBar to which this tab item is added.
        /// </summary>
        public RibbonBar RibbonBar
        {
            get { return ribbon; }
            internal set { ribbon = value; }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RibbonBar.IsExpanded = false;
                RibbonBar.CanMinimize ^= true;
                e.Handled = true;
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        SelectPreviousTab();
                        e.Handled = true;
                        break;

                    case Key.Right:
                        SelectNextTab();
                        e.Handled = true;
                        break;
                }
            }
            base.OnKeyDown(e);
            if (e.Key == Key.Space) RibbonBar.SelectedTabItem = this;
        }

        private StringCollection reductionOrder;

        /// <summary>
        /// Gets or sets the StringCollection with Name of each group to be reducted in that order.
        /// </summary>
        [TypeConverter(typeof(RibbonGroupReductionOrderConverter))]
        public StringCollection ReductionOrder
        {
            get { return reductionOrder; }
            set { reductionOrder = value; }
        }


        /// <summary>
        /// Gets the color for the TabItem.
        /// </summary>
        public Color Color
        {
            get { return tabSet != null ? tabSet.Color : Colors.Transparent; }
        }


        private void SelectNextTab()
        {
            if (IsKeyboardFocused)
            {
                int index = ribbon.IndexOfVisibleTab(ribbon.SelectedTabItem) + 1;
                RibbonTabItem tab = ribbon.VisibleTabFromIndex(index);
                if (tab != null)
                {
                    ribbon.SelectedTabItem = tab;
                    FocusSelectedTab();
                }
            }
        }

        private void FocusSelectedTab()
        {
            RibbonTabItem tab = ribbon.SelectedTabItem;
            if (tab != null) tab.Focus();
        }

        private void SelectPreviousTab()
        {
            if (IsKeyboardFocused)
            {
                int index = ribbon.IndexOfVisibleTab(ribbon.SelectedTabItem) - 1;
                RibbonTabItem tab = ribbon.VisibleTabFromIndex(index);
                if (tab != null)
                {
                    ribbon.SelectedTabItem = tab;
                    FocusSelectedTab();
                }
            }
        }



        #region IKeyboardCommand Members

        void IKeyTipControl.ExecuteKeyTip()
        {
            Focus();
            ribbon.SelectedTabItem = this;
        }

        #endregion
    }
}
