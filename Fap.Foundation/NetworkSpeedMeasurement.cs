#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Foundation
{
    public enum NetSpeedType { Download, Upload };
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
            return (long)(total / time)*1000;
        }


            public class Measurement
            {
                public long Time {set;get;}
                public long Speed {set;get;}
            }
    }
}
