using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class BroadcastService<TOut> : AbstractConnection, IBroadcastService<TOut> where TOut : class
    {
        protected string ExchangeName { get; }

        protected Func<TOut, string> OutDataAdpator;

        public BroadcastService(IConnectionPool connectionPool,
            IChannelPool channelPool, 
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings) 
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            OutDataAdpator = null;

            // Normlize
            ExchangeName = exchange ?? "";
            ExchangeName = ExchangeName.ToUpper();
        }

        public void SetDataAdaptor(Func<TOut, string> f)
        {
            OutDataAdpator = f;
        }

        protected override IBasicProperties BuildChannelProperties(ChannelDecorator channelDecorator)
        {
            var properties = channelDecorator.GetOrCreateProperties((that) =>
            {
                var p = that.Channel.CreateBasicProperties();
                p.Persistent = (bool)Settings["persistent"];
                return p;
            });

            return properties;
        }

        protected override void EnsureExchangeDeclared(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureExchangDeclared(that => channelDecorator.Channel.ExchangeDeclare(ExchangeName, "fanout"));
        }

        public bool BroadcastMessage(TOut data)
        {
            return PublishSafely((channelDecorator) =>
            {
                EnsureExchangeDeclared(channelDecorator);

                var x = OutDataAdpator(data);
                var bytes = Encoding.UTF8.GetBytes(x);

                var props = BuildChannelProperties(channelDecorator);

                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: "",
                                     basicProperties: props,
                                     body: bytes);
            });

        }
    }
}
