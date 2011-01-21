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
        
        private bool EOF = false;
        private bool error = false;
        private BufferService bufferService;

        private AutoResetEvent OnreadEvent = new AutoResetEvent(false);
        private AutoResetEvent OnwriteEvent = new AutoResetEvent(false);

        public BufferedFileReader(BufferService bs)
        {
            bufferService = bs;
        }


        public MemoryBuffer GetBuffer()
        {
            bool wait = false;
            lock (readbuffer)
                wait = readbuffer.Count == 0;
            if (wait)
                OnwriteEvent.WaitOne();
            OnreadEvent.Set();
            return readbuffer.Dequeue();
        }

        private void PutBuffer(MemoryBuffer b)
        {
            lock (readbuffer)
                readbuffer.Enqueue(b);
            OnwriteEvent.Set();
        }

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


        public void Start(FileStream stream)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Run), stream);
        }


        private void Run(object o)
        {
            FileStream stream = o as FileStream;
            try
            {
                long length = stream.Length;
                long position = stream.Position;

                while (position < length)
                {
                    MemoryBuffer arg = bufferService.GetArg();
                    int thisread = stream.Read(arg.Data, 0, arg.Data.Length);
                    arg.SetDataLocation(0, thisread);
                    position += thisread;

                    bool doWait = false;
                    lock (readbuffer)
                    {
                        readbuffer.Enqueue(arg);
                        if (readbuffer.Count > 5)
                            doWait = true;
                    }

                    if (doWait)
                        OnreadEvent.WaitOne();
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
            OnwriteEvent.Set();
        }
    }
}
