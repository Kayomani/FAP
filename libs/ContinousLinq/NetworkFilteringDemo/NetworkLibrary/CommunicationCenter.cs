using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace NetworkLibrary
{
    public static class CommunicationCenter
    {
        public static U OpenPeerChannel<T, U>(T sourceObject,
            string p2pUrl,
            int portNumber) where U : IClientChannel
        {
            InstanceContext sourceContext = new InstanceContext(sourceObject);
            NetPeerTcpBinding binding = new NetPeerTcpBinding()
            {
                Port = portNumber,
                Name = p2pUrl + "@" + portNumber
            };
            binding.Security.Mode = SecurityMode.None;
            EndpointAddress address = new EndpointAddress(p2pUrl);
            DuplexChannelFactory<U> sourceFactory =
                new DuplexChannelFactory<U>(sourceContext, binding, address);
            U sourceProxy = (U)sourceFactory.CreateChannel();
            sourceProxy.Open();
            return sourceProxy;
        }
    }
}
