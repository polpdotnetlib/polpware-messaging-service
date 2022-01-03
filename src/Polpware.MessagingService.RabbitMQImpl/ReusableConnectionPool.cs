using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class ReusableConnectionPool : IConnectionPool
    {
        protected ConcurrentDictionary<string, ConnectionDecorator> Connections { get; }

        protected IConnectionFactoryProvider FactoryProvider { get;  }

        // todo: Do we need to track the state of dispose????
        public ReusableConnectionPool(IConnectionFactoryProvider factoryProvider)
        {
            FactoryProvider = factoryProvider;
            Connections = new ConcurrentDictionary<string, ConnectionDecorator>();
        }

        public ConnectionDecorator Get(string connectionName = null)
        {
            connectionName = connectionName
                                        ?? FactoryProvider.DefaultConnectionName;

            return Connections.GetOrAdd(
                connectionName,
                (k) => FactoryProvider
                    .Build()
                    .CreateConnection()
                    .Map2Decorator(k)
               );
        }

        public void Dispose()
        {
            foreach (var connection in Connections.Values)
            {
                try
                {
                    connection.Dispose();
                }
                catch
                {

                }
            }

            Connections.Clear();
        }
    }
}
