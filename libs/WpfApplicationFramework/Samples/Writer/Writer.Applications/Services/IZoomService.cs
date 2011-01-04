using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Writer.Applications.Services
{
    public interface IZoomService : INotifyPropertyChanged
    {
        IEnumerable<double> DefaultZooms { get; }

        double ActiveZoom { get; set; }
    }
}
