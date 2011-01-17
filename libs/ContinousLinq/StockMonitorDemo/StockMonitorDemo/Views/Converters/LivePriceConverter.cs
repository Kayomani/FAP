using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ContinuousLinq.Aggregates;

namespace StockMonitorDemo.Views.Converters
{
    public class LivePriceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                ContinuousValue<double> cv = (ContinuousValue<double>)value;
                double d = cv.CurrentValue;
                return string.Format("{0:C}", d);
            }
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
