using RabbitMQ.Client;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4DispatchingService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly string _queue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionFactory">RabbitMQ-specific connection factory</param>
        /// <param name="exchange">Exchange name</param>
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues</param>
        /// <param name="queue">Specific queue, or leave it empty so that an anonymous, unique queue is generated</param>
        /// <param name="routingKey">The label used to characterize the group of a message to be sent out.</param>
        public Subscription4DispatchingService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings, string queue, string routingKey) 
            : base(connectionFactory, settings)
        {
            _exchange = exchange;
            _routingKey = routingKey;
            _queue = queue;
        }

        protected override void BuildOrBindQueue()
        {
            existingConnection.Channel.ExchangeDeclare(_exchange, "direct");  // specify more params if needed, like "durable"

            if (string.IsNullOrEmpty(_queue))
            {
                existingConnection.QueueName = existingConnection.Channel.QueueDeclare().QueueName;
            }
            else
            {
                existingConnection.QueueName = existingConnection.Channel.QueueDeclare(_queue, 
                    durable: (bool)Settings["durable"],
                    exclusive: (bool)Settings["exclusive"],
                    autoDelete: (bool)Settings["autoDelete"]).QueueName;
            }

            existingConnection.Channel.QueueBind(queue: existingConnection.QueueName,
                     exchange: _exchange,
                     routingKey: _routingKey);

            // QoS does not make sense for 
            // existingConnection.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

    }
}
