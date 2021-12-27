using RabbitMQ.Client;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4UnicastService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        public Subscription4UnicastService(ConnectionFactory connectionFactory, string queue, IDictionary<string, object> settings) 
            : base(connectionFactory, settings)
        {
            existingConnection.QueueName = queue;
        }

        protected override void BuildOrBindQueue()
        {
            existingConnection.Channel.QueueDeclare(queue: existingConnection.QueueName,
                durable: (bool)_settings["durable"],
                exclusive: (bool)_settings["exclusive"],
                autoDelete: (bool)_settings["autoDelete"],
                arguments: null);

            existingConnection.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

    }
}
