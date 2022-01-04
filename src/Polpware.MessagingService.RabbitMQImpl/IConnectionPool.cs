using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    /// <summary>
    /// The contract for a connection pool
    /// </summary>
    public interface IConnectionPool
    {
        ConnectionDecorator Get(string connectionName = null);
        void Remove(string connectionName);
        void Clear();
    }
}
