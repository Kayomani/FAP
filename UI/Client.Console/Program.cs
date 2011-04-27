using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Server;

namespace Client.Console
{
    class Program
    {
        static void Main(string[] a)
        {
            Program p = new Program();
            p.test();
           
        }


        private void test()
        {
            NodeServer server = new NodeServer();
            server.Start();

            for (int i = 0; i < 100; i++)
            {
                FAP.Network.Client.Client c = new FAP.Network.Client.Client();
                c.Test();
            }
            System.Console.ReadKey();
        }
    }
}
