using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public interface IChannelPool
    {
        IConnectionPool ConnectionPool { get; }
        IModel Acquire(string channelName = null, string connectionName = null);
    }
}
