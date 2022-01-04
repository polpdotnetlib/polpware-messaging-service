using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class OneTimeConnectionPool : IConnectionPool
    {
        protected IConnectionFactoryProvider FactoryProvider { get; }

        public OneTimeConnectionPool(IConnectionFactoryProvider factoryProvider)
        {
            FactoryProvider = factoryProvider;
        }

        public void Dispose()
        {
            // Since we do not track the generated connection, 
            // we cannot have any substantial logic here.            
        }

        public ConnectionDecorator Get(string connectionName = null)
        {
            connectionName = connectionName
                                        ?? FactoryProvider.DefaultConnectionName;
            var factory = FactoryProvider.Build();
            var conn = factory.CreateConnection();
            return new ConnectionDecorator(connectionName, conn);
        }

        public void Remove(string connectionName)
        {
            // Trivial
        }
    }
}
