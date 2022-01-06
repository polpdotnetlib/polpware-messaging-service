using RabbitMQ.Client;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4BroadcastService<TIn, TInter> : SubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {

        public Subscription4BroadcastService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            IDictionary<string, object> settings) 
            : base(connectionPool, channelPool, connectionName, channelName, exchange, settings)
        {
        }

        protected override void EnsureExchangeDeclared(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureExchangDeclared((that) =>
            {
                that.Channel.ExchangeDeclare(ExchangeName, "fanout");  // specify more params if needed, like "durable"
            });
        }

        protected override void BuildOrBindQueue(ChannelDecorator channelDecorator)
        {

            channelDecorator.EnsureQueueBinded((that) =>
            {
                SubscriptionQueueName = channelDecorator.Channel.QueueDeclare(durable: (bool)Settings["durable"],
                        exclusive: (bool)Settings["exclusive"],
                        autoDelete: (bool)Settings["autoDelete"]).QueueName;

                that.Channel.QueueBind(queue: SubscriptionQueueName,
                         exchange: ExchangeName,
                         routingKey: "");
            });

            // QoS does not make sense for 
            // existingConnection.Channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        }
    }
}
