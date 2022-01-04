using RabbitMQ.Client;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    [Obsolete]
    public class ConnectionBuilder : IDisposable
    {
        public IConnection Connection { get; private set; }
        public IModel Channel { get; private set; }

        /// <summary>
        /// Queue name as the result of listening to a topic
        /// </summary>
        public string QueueName { get; set; }

        public ConnectionBuilder(ConnectionFactory connectionFactory)
        {
            Connection = connectionFactory.CreateConnection();
            Channel = Connection.CreateModel();
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Connection?.Dispose();
        }
    }
}
