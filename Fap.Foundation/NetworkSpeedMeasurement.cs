using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Foundation
{
    public class NetworkSpeedMeasurement
    {
        private static readonly long averageSeconds = 5;
        private static readonly long limit = 1000 * averageSeconds;

        List<Measurement> data = new List<Measurement>();


        public void PutData(long size)
        {
            data.Add(new Measurement() { Time = Environment.TickCount, Speed = size });
        }

        public long GetSpeed()
        {
            if (data.Count == 0)
                return 0;

            long total = 0;

            for (int i = data.Count - 1; i >= 0; i--)
            {
                if (data[i].Time < Environment.TickCount - limit)
                {
                    data.RemoveAt(i);
                }
                else
                {
                    total += data[i].Speed;
                }
            }

            double time = Environment.TickCount - data[0].Time;

            if (time == 0)
                time = 1;

            return (long)(total / time)*1000;
        }


            public class Measurement
            {
                public long Time {set;get;}
                public long Speed {set;get;}
            }
    }
}
