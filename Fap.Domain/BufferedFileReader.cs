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
using System.IO;
using System.Threading;
using Fap.Domain.Services;
using System.Net.Sockets;
using Fap.Domain.Entity;
using Fap.Foundation;
using Fap.Network.Services;

namespace Fap.Domain
{
    public class BufferedFileReader
    {
        public Queue<MemoryBuffer> readbuffer = new Queue<MemoryBuffer>();
        public delegate void ReadAsync(string path, long resumePoint);

        private bool EOF = false;
        private bool error = false;
        private BufferService bufferService;

        public BufferedFileReader(BufferService bs)
        {
            bufferService = bs;
        }

        public MemoryBuffer LastBuffer { set; get; }

        public bool IsEOF
        {
            set { lock (readbuffer) { EOF = value; } }
            get { lock (readbuffer) { return EOF; } }
        }

        public bool HasError
        {
            set { lock (readbuffer) { error = value; } }
            get { lock (readbuffer) { return error; } }
        }


        public void Start(string path, long resumePoint)
        {
            ReadAsync ra = new ReadAsync(Run);
            ra.BeginInvoke(path,resumePoint, null, null);
        }


        private void Run(string path, long resumePoint)
        {
            try
            {
                using (Stream fs = new FileStream(path, FileMode.Open,FileAccess.Read,FileShare.Read))
                {
                    long totalread = resumePoint;
                    int wait = 5;

                    if (resumePoint < fs.Length && resumePoint!=0)
                    {
                        fs.Seek(resumePoint, SeekOrigin.Begin);
                    }

                    while (fs.Length > totalread)
                    {
                        MemoryBuffer arg = bufferService.GetArg();
                        int thisread = fs.Read(arg.Data, 0, arg.Data.Length);
                        arg.SetDataLocation(0, thisread);
                        totalread += thisread;
                        bool doWait = false;

                        lock (readbuffer)
                        {
                            readbuffer.Enqueue(arg);
                            if (readbuffer.Count > 5)
                                doWait = true;
                            wait = 5;
                        }

                        while (doWait)
                        {
                            Thread.Sleep(wait);
                            lock (readbuffer)
                            {
                                doWait = (readbuffer.Count > 5);
                                if (wait < 50)
                                    wait += 5;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.ToString();
                lock (readbuffer)
                {
                    HasError = true;
                    while (readbuffer.Count > 0)
                    {
                       bufferService.FreeArg(readbuffer.Dequeue());
                    }
                }

            }
            IsEOF = true;
        }
    }
}
