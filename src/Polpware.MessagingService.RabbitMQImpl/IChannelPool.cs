using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public interface IChannelPool
    {
        IConnectionPool ConnectionPool { get; }
        ChannelDecorator Get(string channelName = null, string connectionName = null);
        void Remove(string channelName, string connectionName);
        // todo: Maybe clear by connection name
        void Clear();
    }
}
