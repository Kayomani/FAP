using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

#region Copyright
// Odyssey.Controls.Ribbonbar
// (c) copyright 2009 Thomas Gerber
// This source code and files, is licensed under The Microsoft Public License (Ms-PL)
#endregion
namespace Odyssey.Controls
{
    partial class RibbonBar
    {
        public static readonly RoutedUICommand AlignGroupsLeftCommand = new RoutedUICommand("Align Left", "AlignGroupsLeftCommand", typeof(RibbonBar));
        public static readonly RoutedUICommand AlignGroupsRightCommand = new RoutedUICommand("Align Right", "AlignGroupsRightCommand", typeof(RibbonBar));
        public  static readonly RoutedUICommand CollapseRibbonBarCommand = new RoutedUICommand("", "CollapseRibbonBarCommand", typeof(RibbonBar));

        public static readonly RoutedUICommand QAPlacementTopCommand = new RoutedUICommand("Show Above the Ribbon.", "QAPlacementTopCommand", typeof(RibbonBar));
        public static readonly RoutedUICommand QAPlacementBottomCommand = new RoutedUICommand("Show Below the Ribbon.", "QAPlacementBottomCommand", typeof(RibbonBar));

        private static void RegisterCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(RibbonBar), new CommandBinding(AlignGroupsLeftCommand, alignGroupsLeft));
            CommandManager.RegisterClassCommandBinding(typeof(RibbonBar), new CommandBinding(AlignGroupsRightCommand, alignGroupsRight));
            CommandManager.RegisterClassCommandBinding(typeof(RibbonBar), new CommandBinding(CollapseRibbonBarCommand, collapseRibbonBar));

            CommandManager.RegisterClassCommandBinding(typeof(RibbonBar), new CommandBinding(QAPlacementTopCommand, QAPlacementTop, IsQAPlacementTopEnabled));
            CommandManager.RegisterClassCommandBinding(typeof(RibbonBar), new CommandBinding(QAPlacementBottomCommand, QAPlacementBottom, IsQAPlacementBottomEnabled));
        }

        private static void QAPlacementTop(object sender, ExecutedRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            bar.ToolbarPlacement = QAPlacement.Top;
        }

        private static void QAPlacementBottom(object sender, ExecutedRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            bar.ToolbarPlacement = QAPlacement.Bottom;
        }

        private static void IsQAPlacementTopEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            e.CanExecute = bar.ToolbarPlacement == QAPlacement.Bottom;
        }

        private static void IsQAPlacementBottomEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            e.CanExecute = bar.ToolbarPlacement == QAPlacement.Top;
        }

        private static void collapseRibbonBar(object sender, ExecutedRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            bar.IsExpanded = false;

        }

        private static void alignGroupsLeft(object sender, ExecutedRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            bar.AlignGroupsLeft();
       }


        private static void alignGroupsRight(object sender, ExecutedRoutedEventArgs e)
        {
            RibbonBar bar = (RibbonBar)sender;
            bar.AlignGroupsRight();
        }
    }
}
