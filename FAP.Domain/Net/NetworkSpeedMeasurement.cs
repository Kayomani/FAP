using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Net
{
   
    public class NetworkSpeedMeasurement
    {
        private static readonly long averageSeconds = 5;
        private static readonly long limit = 1000 * averageSeconds;
        public static readonly NetworkSpeedMeasurement TotalUpload = new NetworkSpeedMeasurement(NetSpeedType.Upload);
        public static readonly NetworkSpeedMeasurement TotalDownload = new NetworkSpeedMeasurement(NetSpeedType.Download);
        private NetSpeedType type;

        private object sync = new object();

        public NetworkSpeedMeasurement(NetSpeedType t)
        {
            type = t;
        }

        List<Measurement> data = new List<Measurement>();


        public void PutData(long size)
        {
            lock (sync)
                data.Add(new Measurement() { Time = Environment.TickCount, Speed = size });

            if (this == TotalDownload || this == TotalUpload)
                return;

            if (type == NetSpeedType.Download)
                TotalDownload.PutData(size);
            else
                TotalUpload.PutData(size);
        }

        public long GetSpeed()
        {
            long total = 0;
            double time = 0;
            lock (sync)
            {
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

                if (data.Count == 0)
                    return 0;

                time = Environment.TickCount - data[0].Time;

                if (time == 0)
                    time = 1;
            }
            return (long)(total / time) * 1000;
        }


        public class Measurement
        {
            public long Time { set; get; }
            public long Speed { set; get; }
        }
    }
}
