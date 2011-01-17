using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ContinuousLinq;
using StockMonitorDemo.Models;
using System.Windows.Media;
using System.Windows;

namespace StockMonitorDemo.Views.Converters
{
    public class TickCollectionToPointCollectionConverter : IValueConverter
    {
        #region IValueConverter Members
        private const double PreviousClosingPoint = 100.0;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            double ActualWidth = 300;
            double ActualHeight = 270;

            ContinuousCollection<StockSaleTick> tickCollection =
                (ContinuousCollection<StockSaleTick>)value;

            if (tickCollection.Count == 0)
                return null;

            double xConst = -tickCollection.First().TimeStamp.Ticks;
            double xScale = ActualWidth / (tickCollection.Last().TimeStamp.Ticks + xConst);

            double yMin = Math.Min(PreviousClosingPoint, tickCollection.Min(t=> t.Price) - 0.1);
            double yMax = Math.Max(PreviousClosingPoint, tickCollection.Max(t=> t.Price) + 0.1);
            double yConst = -yMax;
            double yScale = ActualHeight / (yMin + yConst);

            Func<double, double> yConvert = tickPrice => yScale * (tickPrice + yConst);
            Func<double, double> xConvert = tickTime => xScale * (tickTime + xConst);
            Func<StockSaleTick, Point> ptConvert = tick => new Point(xConvert(tick.TimeStamp.Ticks),
                                                                            yConvert(tick.Price));


            PointCollection points = new PointCollection();

            for (int i = 0; i < tickCollection.Count; i++)
            {
                Point pt = ptConvert(tickCollection[i]);
                points.Add(pt);
            }

            return points;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
