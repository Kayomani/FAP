using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using BookLibrary.Applications.Controllers;

namespace BookLibrary.Presentation
{
    [Export(typeof(IPresentationController))]
    public class PresentationController : IPresentationController
    {
        public void InitializeCultures()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
