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

       public byte[] Data { set; get; }
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

       public Socket Socket { set; get; }

       public MemoryBuffer(int size)
       {
           Data = new byte[size];
       }

       public void Dispose()
       {
           Data = new byte[0];
           Socket = null;
       }
    }
}
