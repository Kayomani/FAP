#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
    public class StreamTokenizer
    {
        private readonly string TERMINATOR = "\r\n";
        private List<MemoryBuffer> buffers = new List<MemoryBuffer>();

        private Encoding encoding;

        public StreamTokenizer()
        {
            encoding = Encoding.ASCII;
        }


        public StreamTokenizer(Encoding e, string splitter)
        {
            encoding = e;
            TERMINATOR = splitter;
        }

        public long InputBufferLength
        {
            get { return buffers.Count; }
        }

        public void Dispose()
        {
            buffers.Clear();
            buffers = null;
        }

        public string InputBuffer
        {
            get
            {
                if (null == buffers)
                    return string.Empty;
                if (buffers.Count == 1)
                {
                    return encoding.GetString(buffers[0].Data, 0, buffers[0].DataSize);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (MemoryBuffer b in buffers)
                        sb.Append(encoding.GetString(b.Data, 0, b.DataSize));
                    return sb.ToString();
                }
            }
        }

        public List<MemoryBuffer> Buffers
        {
            get { return buffers; }
        }

        public void ResetInputBuffer() { buffers = new List<MemoryBuffer>(); }

        /// <summary>
        /// Scan for token
        /// </summary>
        /// <returns></returns>
        public bool ContainsCommand()
        {
            if (buffers.Count == 0)
                return false;
            if (buffers.Count == 1)
                return encoding.GetString(buffers[0].Data, buffers[0].StartLocation, buffers[0].DataSize).Contains(TERMINATOR);

            StringBuilder sb = new StringBuilder();

            long length = 0;
            foreach (var buffer in buffers)
            {
                sb.Append(encoding.GetString(buffer.Data, buffer.StartLocation, buffer.DataSize));
                if (sb.ToString().Contains(TERMINATOR))
                    return true;
                length += buffer.DataSize;
                //Hard limit at 10mb for sanity
                if (length > 102400000)
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Returns the top token - The buffer may contain multiple tokens.
        /// </summary>
        /// <returns></returns>
        public string GetCommand()
        {
            if (buffers.Count == 1)
            {
                string msg = encoding.GetString(buffers[0].Data, buffers[0].StartLocation, buffers[0].DataSize);
                int endIndex = msg.IndexOf(TERMINATOR);
                if (endIndex + TERMINATOR.Length != msg.Length)
                {
                    //We have a partial bit of the next request so leave that in the buffer.
                    if (encoding == Encoding.ASCII)
                    {
                        string substring = msg.Substring(0, endIndex);
                        buffers[0].SetDataLocation(buffers[0].StartLocation + endIndex + TERMINATOR.Length, buffers[0].DataSize - (endIndex + TERMINATOR.Length));
                        return substring;
                    }
                    else
                    {
                        //We may have a multi byte terminator so work out the size.
                        string substring = msg.Substring(0,endIndex + TERMINATOR.Length);
                        byte[] data = encoding.GetBytes(substring);
                        buffers[0].SetDataLocation(buffers[0].StartLocation + data.Length, buffers[0].DataSize - data.Length);
                        data = null;
                        return substring;
                    }
                }
                else
                {
                    buffers.RemoveAt(0);
                    return msg.Substring(0,msg.Length-TERMINATOR.Length);
                }
            }

            StringBuilder sb = new StringBuilder();
            List<MemoryBuffer> processed = new List<MemoryBuffer>();

            foreach (MemoryBuffer b in buffers)
            {
                string msg = encoding.GetString(b.Data, b.StartLocation, b.DataSize);
                if (msg.Contains(TERMINATOR))
                {
                    int endIndex = msg.IndexOf(TERMINATOR);
                    if (endIndex + TERMINATOR.Length != msg.Length)
                    {
                        //We have a partial bit of the next request so leave that in the buffer.
                        string substring = msg.Substring(0, endIndex + TERMINATOR.Length);
                        byte[] data = encoding.GetBytes(substring);
                        b.SetDataLocation(b.StartLocation + data.Length, b.DataSize - data.Length);
                        data = null;
                        sb.Append(substring);
                    }
                    else
                    {
                        sb.Append(msg);
                        processed.Add(b);
                    }
                    break;
                }
                else
                {
                    processed.Add(b);
                    sb.Append(msg);
                }
            }
            foreach (var buff in processed)
                buffers.Remove(buff);
            return sb.ToString();
        }

        public void ReceiveData(MemoryBuffer buff)
        {
            buffers.Add(buff);
        }
    }
}
