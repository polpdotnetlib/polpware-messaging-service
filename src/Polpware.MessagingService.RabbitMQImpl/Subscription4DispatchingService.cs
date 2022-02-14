using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4DispatchingService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        protected string RoutingKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionPool">Connection pool</param>
        /// <param name="channelPool">Channel pool</param>
        /// <param name="connectionName">Connection name</param>
        /// <param name="channelName">Channel name</param>
        /// <param name="exchange">Exchange name</param> 
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues</param>
        /// <param name="queue">Specific queue, or leave it empty so that an anonymous, unique queue is generated</param>
        /// <param name="routingKey">The label used to characterize the group of a message to be sent out.</param>
        public Subscription4DispatchingService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings, string queue, string routingKey) 
            : base(connectionPool, channelPool, connectionName, channelName, exchange, settings)
        {
            SetQueueAndRoutingKey(queue, routingKey);
        }

        public Subscription4DispatchingService(IConnectionPool connectionPool,
            IChannelPool channelPool)
            : base(connectionPool, channelPool)
        {
        }

        protected void SetQueueAndRoutingKey(string queue, string routingKey)
        {
            RoutingKey = routingKey.ToUpper();

            SubscriptionQueueName = string.IsNullOrEmpty(queue) ? Guid.NewGuid().ToString().ToUpper() : queue.ToUpper();
        }

        protected override void EnsureExchangeDeclared(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureExchangDeclared((that) =>
            {
                that.Channel.ExchangeDeclare(ExchangeName, "direct");  // specify more params if needed, like "durable"
            });
        }

        protected override void BuildOrBindQueue(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureQueueBinded(SubscriptionQueueName, (that) =>
            {
                that.Channel.QueueDeclare(SubscriptionQueueName,
                    durable: (bool)Settings["durable"],
                    exclusive: (bool)Settings["exclusive"],
                    autoDelete: (bool)Settings["autoDelete"]);

                that.Channel.QueueBind(queue: SubscriptionQueueName,
                         exchange: ExchangeName,
                         routingKey: RoutingKey);
            });

            // QoS does not make sense for 
            // existingConnection.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }

    }
}
