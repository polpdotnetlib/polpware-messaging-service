using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    /// <summary>
    /// The contract for a connection pool
    /// </summary>
    public interface IConnectionPool : IDisposable
    {
        ConnectionDecorator Get(string connectionName = null);
    }
}
