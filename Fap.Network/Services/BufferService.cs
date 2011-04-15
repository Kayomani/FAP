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
using Fap.Foundation;
using NLog;

namespace Fap.Network.Services
{
    public class BufferService
    {
        private Stack<MemoryBuffer> pool = new Stack<MemoryBuffer>();
        private Stack<MemoryBuffer> smallPool = new Stack<MemoryBuffer>();
        private Logger logger;

        public const int Buffer = 2621440;//2mb
        public const int SmallBuffer = 256000;//256kb

        private int largeCount = 10;
        private int smallCount = 10;

        public BufferService()
        {
            logger = LogManager.GetLogger("faplog");
        }

        public void Clean()
        {
            lock (pool)
            {
                while (pool.Count > largeCount)
                {
                    MemoryBuffer arg = pool.Pop();
                    arg.Dispose();
                }
            }
            lock (smallPool)
            {
                while (smallPool.Count > smallCount)
                {
                    MemoryBuffer arg = smallPool.Pop();
                    arg.Dispose();
                }
            }
        }


        public void Setup(int smallCount, int largeCount)
        {
            lock (smallPool)
            {
                this.smallCount = smallCount;
                for (int i = 0; i < smallCount; i++)
                {
                    MemoryBuffer a = new MemoryBuffer(SmallBuffer);
                    smallPool.Push(a);
                }
            }
            lock (pool)
            {
                this.largeCount = largeCount;
                for (int i = 0; i < largeCount; i++)
                {
                    MemoryBuffer a = new MemoryBuffer(Buffer);
                    pool.Push(a);
                }
            }
        }



        public MemoryBuffer GetArg()
        {
            lock (pool)
            {
                if (pool.Count > 0)
                    return pool.Pop();
            }
            MemoryBuffer a = new MemoryBuffer(Buffer);
            return a;
        }

        public MemoryBuffer GetSmallArg()
        {
            lock (smallPool)
            {
                if (smallPool.Count > 0)
                    return smallPool.Pop();
            }
            MemoryBuffer a = new MemoryBuffer(SmallBuffer);
            return a;
        }

        public void FreeArg(MemoryBuffer input)
        {
            input.SetDataLocation(0, 0);
            if (input.Data.Length == SmallBuffer)
            {
                lock (smallPool)
                    smallPool.Push(input);
            }
            else if (input.Data.Length == Buffer)
            {
                lock (pool)
                    pool.Push(input);
            }
            else
            {
                logger.Warn("Tried to free incorrectly sized arg with length: {0}", input.Data.Length);
            }
        }
    }
}
