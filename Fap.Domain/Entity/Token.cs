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
using System.Net.Sockets;
using Fap.Foundation;

namespace Fap.Domain.Entity
{
    /// <summary>
    /// Token for use with SocketAsyncEventArgs.
    /// </summary>
    public sealed class Token
    {
        private List<MemoryBuffer> inputBuffer = new List<MemoryBuffer>();

        private string outputData = null;
        private int outputDataLocation = 0;


        public void Dispose()
        {
            inputBuffer.Clear();
            inputBuffer = null;
            outputData = null;
        }

        public long InputBufferLength
        {
            get { return inputBuffer.Count; }
        }

        public string InputBuffer
        {
            get
            {
                if (null == inputBuffer)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();

                foreach (MemoryBuffer b in inputBuffer)
                    sb.Append(Encoding.ASCII.GetString(b.Data, 0, b.DataSize));

                return sb.ToString();
            }
        }

        public MemoryBuffer[] InputBufferBytes
        {
            get { return inputBuffer.ToArray(); }
        }

        public void SetOutputData(string data)
        {
            outputData = data;
        }

        public void ResetInputBuffer() { inputBuffer = new List<MemoryBuffer>(); }
        public void ResetOutputBuffer() { outputData = null; }



        public bool ContainsCommand()
        {
          /*  if (inputBuffer.Count == 0)
                return false;

            if (inputBuffer[0].DataSize > 0 && inputBuffer[0].Data.Length > 0)
            {
                if (inputBuffer[0].Data[0] == Mediator.START_CHAR)
                {
                    //We have a start char so search for the end char
                    foreach (MemoryBuffer b in inputBuffer)
                    {
                        for (int i = 0; i < b.DataSize; i++)
                        {
                            if (b.Data[i] == Mediator.END_CHAR)
                                return true;
                        }
                    }

                }
            }*/
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
            foreach (MemoryBuffer b in inputBuffer.ToList())
            {
                int index = -1;
                //Check buffer for endtag
                for (int i = 0; i < b.DataSize; i++)
                {
                   // if (b.Data[i] == Mediator.END_CHAR)
                    {
                        index = i+1;
                        break;
                    }
                }


                if (-1 == index)
                    sb.Append(Encoding.ASCII.GetString(b.Data, 0, b.DataSize));
                else
                {
                    sb.Append(Encoding.ASCII.GetString(b.Data, 0, index));

                    if (index == b.DataSize)
                        processed.Add(b);
                    else
                    {
                        b.SetDataLocation(index, b.DataSize - index);
                    }

                    foreach (var buff in processed)
                        inputBuffer.Remove(buff);
                    return sb.ToString();

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

        public bool SendData(MemoryBuffer arg)
        {
            if (outputData.Length == 0)
                return false;
            int packetLength = outputData.Length - outputDataLocation;
            if (packetLength > 0)
            {
                if (packetLength > arg.Data.Length)
                    packetLength = arg.Data.Length;
                int len = Encoding.ASCII.GetBytes(outputData, outputDataLocation, packetLength, arg.Data, 0);
                arg.SetDataLocation(0,len);
                outputDataLocation += packetLength;
                return true;
            }
            return false;
        }

    }
}
