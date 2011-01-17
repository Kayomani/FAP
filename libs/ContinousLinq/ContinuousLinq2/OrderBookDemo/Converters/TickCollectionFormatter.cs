using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContinuousLinq.OrderBookDemo.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace ContinuousLinq.OrderBookDemo.Converters
{
    public class TickCollectionToPointCollectionConverter : IValueConverter
    {
        #region IValueConverter Members
        private const double PreviousClosingPrice = 100.0;
        private static double _fiveMinuteWindow;

        static TickCollectionToPointCollectionConverter()
        {
            _fiveMinuteWindow = TimeSpan.FromMinutes(5).Ticks;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            double ActualWidth = 400;
            double ActualHeight = 160; 
            

            ReadOnlyContinuousCollection<StockTransaction> tickCollection =
                (ReadOnlyContinuousCollection<StockTransaction>)value;

            if (tickCollection.Count == 0)
                return null;

            double priceMin = Math.Min(PreviousClosingPrice, tickCollection.Min(t => t.Price));
            double priceMax = Math.Max(PreviousClosingPrice, tickCollection.Max(t => t.Price));

            double tickMin = tickCollection.Min(t => t.TimeStamp.Ticks);
            double tickMax = tickCollection.Max(t => t.TimeStamp.Ticks);

            double yScale = ActualHeight/(priceMax-priceMin);
            double xScale = ActualWidth / Math.Min((tickMax-tickMin), _fiveMinuteWindow);

            StockTransaction[] querySource = tickCollection.ToArray(); // turn CLINQ off
            var newPoints =
                from tick in querySource
                select new Point
                {
                    X = (tick.TimeStamp.Ticks - tickMin) * xScale,
                    Y = (ActualHeight - (tick.Price - priceMin) * yScale)
                };

            return new PointCollection(newPoints.ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
