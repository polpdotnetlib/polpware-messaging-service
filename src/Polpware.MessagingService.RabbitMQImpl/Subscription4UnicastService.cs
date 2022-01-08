using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4UnicastService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        public Subscription4UnicastService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            string queue, 
            IDictionary<string, object> settings) 
            : base(connectionPool, channelPool, connectionName, channelName, exchange, settings)
        {
            // Normalize
            SubscriptionQueueName = string.IsNullOrEmpty(queue) ? Guid.NewGuid().ToString().ToUpper() : queue.ToUpper();
        }

        protected override void EnsureExchangeDeclared(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureExchangDeclared((that) =>
            {
                if (!string.IsNullOrEmpty(ExchangeName))
                {
                    that.Channel.ExchangeDeclare(ExchangeName, "direct");  // specify more params if needed, like "durable"
                }
            });
        }

        protected override void BuildOrBindQueue(ChannelDecorator channelDecorator)
        {

            channelDecorator.EnsureQueueBinded(SubscriptionQueueName, (that) =>
            {

                that.Channel.QueueDeclare(queue: SubscriptionQueueName,
                    durable: (bool)Settings["durable"],
                    exclusive: (bool)Settings["exclusive"],
                    autoDelete: (bool)Settings["autoDelete"],
                    arguments: null);

                // todo: Do we need the binding?
                that.Channel.QueueBind(queue: SubscriptionQueueName,
                         exchange: ExchangeName,
                         routingKey: SubscriptionQueueName);

                that.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            });


        }

    }
}
