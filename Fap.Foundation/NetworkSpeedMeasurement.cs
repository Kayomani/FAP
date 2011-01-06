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
