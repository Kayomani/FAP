using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using System.Waf.Applications;
using BookLibrary.Applications.Services;
using System.ComponentModel.Composition;

namespace BookLibrary.Presentation.Services
{
    [Export(typeof(IQuestionService))]
    public class QuestionService : IQuestionService
    {
        private static MessageBoxOptions MessageBoxOptions
        {
            get
            {
                return (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft) ? MessageBoxOptions.RtlReading : MessageBoxOptions.None;
            }
        }


        public bool? ShowQuestion(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question, MessageBoxResult.Cancel, MessageBoxOptions);

            if (result == MessageBoxResult.Yes) { return true; }
            else if (result == MessageBoxResult.No) { return false; }

            return null;
        }

        public bool ShowYesNoQuestion(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, ApplicationInfo.ProductName, MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions);

            return result == MessageBoxResult.Yes;
        }
    }
}
