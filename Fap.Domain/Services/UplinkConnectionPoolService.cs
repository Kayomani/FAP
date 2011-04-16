using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Fap.Foundation;

namespace Fap.Domain.Services
{
    public class UplinkConnectionPoolService
    {
        private BackgroundSafeObservable<UplinkConnection> list = new BackgroundSafeObservable<UplinkConnection>();

        public void AddConnection(Socket s, string id)
        {
            list.Add(new UplinkConnection() { ID = id, Socket = s, TimeAdded = Environment.TickCount });
        }

        public Socket FindUplink(string id)
        {
            var item = list.Where(i => i.ID == id).FirstOrDefault();
            if (null != item)
                list.Remove(item);
            return item.Socket;
        }

        public void CleanUp()
        {
            if (list.Count > 0)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].TimeAdded + 60000 < Environment.TickCount)
                    {
                        //Item has been pooled for more than 60 seconds, it must be a connection in error so kill it.
                        try
                        {
                            var item = list[i];
                            list.RemoveAt(i);
                            if (item.Socket != null)
                            {
                                if (item.Socket.Connected)
                                    item.Socket.Disconnect(true);
                                item.Socket.Close(0);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public class UplinkConnection
        {
            public string ID { set; get; }
            public Socket Socket { set; get; }
            public long TimeAdded { set; get; }
        }
    }
}
