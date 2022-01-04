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

            var item = Connections.GetOrAdd(
                connectionName,
                (k) => FactoryProvider
                    .Build()
                    .CreateConnection()
                    .Map2Decorator(k)
               );

            if (item.IsDisposed || !item.IsOpen)
            {
                throw new UnexpectedConnectionDecoratorException();
            }

            return item;
        }

        public void Clear()
        {
            foreach (var entry in Connections.Values)
            {
                entry.Close();
                entry.Dispose();
            }

            Connections.Clear();
        }

        public void Remove(string connectionName)
        {
            Connections.TryRemove(connectionName, out ConnectionDecorator entry);
            if (entry != null)
            {
                // Should not throw any exception
                entry.Close();
                entry.Dispose();
            } 
        }
    }
}
