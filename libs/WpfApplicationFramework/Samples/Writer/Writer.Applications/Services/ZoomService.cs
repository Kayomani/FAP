using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Foundation;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;

namespace Writer.Applications.Services
{
    [Export(typeof(IZoomService))]
    public class ZoomService : Model, IZoomService
    {
        private readonly ReadOnlyCollection<double> readOnlyDefaultZooms;
        private double activeZoom;


        [ImportingConstructor]
        public ZoomService()
        {
            readOnlyDefaultZooms = new ReadOnlyCollection<double>(new double[]
            {
                2, 1.5, 1.25, 1, 0.75, 0.5,
            });
            activeZoom = 1;
        }


        public IEnumerable<double> DefaultZooms { get { return readOnlyDefaultZooms; } }

        public double ActiveZoom
        {
            get { return activeZoom; }
            set
            {
                if (activeZoom != value)
                {
                    activeZoom = value;
                    RaisePropertyChanged("ActiveZoom");
                }
            }
        }
    }
}
