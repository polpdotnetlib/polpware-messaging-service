using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class DispatchingService<TOut> : AbstractConnection, IDispatchingService<TOut> where TOut : class
    {
        protected readonly string ExchangeName;
        protected IBasicProperties Props;

        protected Func<TOut, string> OutDataAdpator;

        public DispatchingService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings)
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            OutDataAdpator = null;

            // Normalize
            ExchangeName = exchange ?? "";
            ExchangeName = ExchangeName.ToUpper();
        }

        public void SetDataAdaptor(Func<TOut, string> f)
        {
            OutDataAdpator = f;
        }

        /// <summary>
        /// Properties to be used for sending out messages.
        /// </summary>
        protected override IBasicProperties BuildChannelProperties(ChannelDecorator channelDecorator)
        {
            var p = channelDecorator.GetOrCreateProperties((that) =>
            {
                var properties = that.Channel.CreateBasicProperties();
                properties.Persistent = (bool)Settings["persistent"];
                return properties;
            });

            return p;
        }

        protected override void EnsureExchangeDeclared(ChannelDecorator channelDecorator)
        {
            channelDecorator.EnsureExchangDeclared(that =>
            {
                that.Channel.ExchangeDeclare(ExchangeName, "direct");
            });
        }

        public virtual bool DispatchMessage(TOut data, string routingKey)
        {
            routingKey = routingKey.ToUpper();

            return PublishSafely((channelDecorator) =>
            {

                EnsureExchangeDeclared(channelDecorator);

                var x = OutDataAdpator(data);
                var bytes = System.Text.Encoding.UTF8.GetBytes(x);

                var props = BuildChannelProperties(channelDecorator);

                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: Props,
                                     body: bytes);
            });
        }
    }
}
