using RabbitMQ.Client;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4BroadcastService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        private readonly string _exchange;
        public Subscription4BroadcastService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings) 
            : base(connectionFactory, settings)
        {
            _exchange = exchange;
        }

        protected override void BuildOrBindQueue()
        {
            existingConnection.Channel.ExchangeDeclare(_exchange, "fanout");  // specify more params if needed, like "durable"

            existingConnection.QueueName = existingConnection.Channel.QueueDeclare().QueueName;

            existingConnection.Channel.QueueBind(queue: existingConnection.QueueName,
                     exchange: _exchange,
                     routingKey: "");

            // QoS does not make sense for 
            // existingConnection.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }
    }
}
