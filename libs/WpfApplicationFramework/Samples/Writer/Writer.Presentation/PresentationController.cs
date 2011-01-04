using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Controllers;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;

namespace Writer.Presentation
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
