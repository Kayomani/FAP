using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkLibrary;
using System.Configuration;
using System.Timers;

namespace TransactionSender
{
    class Program
    {
        private static LocalTransactionService _txService = new LocalTransactionService();
        private static ITransactionServiceChannel _txChannel;

        static void Main(string[] args)
        {
            _txChannel = 
                CommunicationCenter.OpenPeerChannel<ITransactionService, ITransactionServiceChannel>(
                _txService, "net.p2p://clinq/netfilterdemo",
                Int32.Parse(ConfigurationManager.AppSettings["peerPort"]));

            Timer t = new Timer();
            t.Interval = 4000;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Enabled = true;
            Console.WriteLine("Transaction Sender is running. [Enter to Quit]");
            Console.ReadLine();
        }

        static void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Publishing batch of 3 TX.");
            _txChannel.PublishTransaction(
                "WAREHOUSE1",
                150.00,
                "Kevin",
                300,
                "10115059123");

            _txChannel.PublishTransaction(
                "WAREHOUSE2",
                300.00,
                "Erb",
                450,
                "1039321209312");

            _txChannel.PublishTransaction(
                "WAREHOUSE3",
                123.32,
                "Bob",
                123,
                "490328400321");
        }
    }
}
