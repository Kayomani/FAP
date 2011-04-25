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

namespace Fap.Network
{
    public class ConnectionToken
    {
        private static readonly string TERMINATOR = "\n\n";
        private List<MemoryBuffer> inputBuffer = new List<MemoryBuffer>();

        public long InputBufferLength
        {
            get { return inputBuffer.Count; }
        }

        public void Dispose()
        {
            inputBuffer.Clear();
            inputBuffer = null;
        }

        public string InputBuffer
        {
            get
            {
                if (null == inputBuffer)
                    return string.Empty;
                if (inputBuffer.Count == 1)
                {
                    return Encoding.Unicode.GetString(inputBuffer[0].Data, 0, inputBuffer[0].DataSize);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (MemoryBuffer b in inputBuffer)
                        sb.Append(Encoding.Unicode.GetString(b.Data, 0, b.DataSize));
                    return sb.ToString();
                }
            }
        }

        public List<MemoryBuffer> RawInputBuffers
        {
            get { return inputBuffer; }
        }

        public void ResetInputBuffer() { inputBuffer = new List<MemoryBuffer>(); }

        /// <summary>
        /// Scan for two consecutive new line characters.  Unicode = 10,0,10,0
        /// </summary>
        /// <returns></returns>
        public bool ContainsCommand()
        {
            if (inputBuffer.Count == 0)
                return false;
            if (inputBuffer.Count == 1)
                return Encoding.Unicode.GetString(inputBuffer[0].Data, inputBuffer[0].StartLocation, inputBuffer[0].DataSize).Contains(TERMINATOR);
            StringBuilder sb = new StringBuilder();

            long length = 0;
            foreach (var buffer in inputBuffer)
            {
                sb.Append(Encoding.Unicode.GetString(buffer.Data, buffer.StartLocation, buffer.DataSize));
                if (sb.ToString().Contains(TERMINATOR))
                    return true;
                length += buffer.DataSize;

                //Limit at 10mb.
                if (length > 102400000)
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Returns the top command - The buffer may have multiple commands.
        /// </summary>
        /// <returns></returns>
        public string GetCommand()
        {
            StringBuilder sb = new StringBuilder();
            List<MemoryBuffer> processed = new List<MemoryBuffer>();

            if (inputBuffer.Count == 1)
            {
                string msg = Encoding.Unicode.GetString(inputBuffer[0].Data, inputBuffer[0].StartLocation, inputBuffer[0].DataSize);
                int endIndex = msg.IndexOf(TERMINATOR);
                if (endIndex + 2 != msg.Length)
                {
                    //We have a partial bit of the next request so leave that in the buffer.
                    string substring = msg.Substring(0, endIndex + 2);
                    byte[] data = Encoding.Unicode.GetBytes(substring);
                    inputBuffer[0].SetDataLocation(inputBuffer[0].StartLocation + data.Length, inputBuffer[0].DataSize - data.Length);
                    data = null;
                    return substring;
                }
                else
                {
                    inputBuffer.RemoveAt(0);
                    return msg;
                }
            }

            foreach (MemoryBuffer b in inputBuffer.ToList())
            {
                string msg = Encoding.Unicode.GetString(b.Data, b.StartLocation, b.DataSize);
                if (msg.Contains(TERMINATOR))
                {
                   
                    int endIndex = msg.IndexOf(TERMINATOR);
                    if (endIndex + 2 != msg.Length)
                    {
                        //We have a partial bit of the next request so leave that in the buffer.
                        string substring = msg.Substring(0,endIndex + 2);
                        byte[] data = Encoding.Unicode.GetBytes(substring);
                        b.SetDataLocation(b.StartLocation+data.Length, b.DataSize-data.Length);
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
                inputBuffer.Remove(buff);
            return sb.ToString();
        }

        public void ReceiveData(MemoryBuffer buff)
        {
            inputBuffer.Add(buff);
        }
    }
}
