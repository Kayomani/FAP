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

namespace Fap.Foundation
{
   public class MemoryBuffer
    {
       private int dataSize;
       private int startLocation;
       private byte[] data;

       public MemoryBuffer(byte[] d)
       {
           data = d;
       }

       public MemoryBuffer(int size)
       {
           data = new byte[size];
       }

       public byte[] Data
       {
           get { return data; }
       }

       public int DataSize
       {
           get { return dataSize; }
       }

       public int StartLocation
       {
           get { return startLocation; }
       }

       public void SetDataLocation(int start, int size)
       {
           dataSize = size;
           startLocation = start;
           if (null == Data || start + size > Data.Length)
               throw new Exception("Unset / Incorrectly sized buffer");
       }

       public void Dispose()
       {
           data = new byte[0];
       }
    }
}
