using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class UnicastService<T> : AbstractConnection, IUnicastService<T> where T : class
    {
        protected readonly string ExchangeName;
        protected readonly string QueueName;

        protected Func<T, object> OutDataAdpator;

        public UnicastService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            string queue, 
            IDictionary<string, object> settings) 
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            OutDataAdpator = id => id;

            ExchangeName = exchange ?? "";
            ExchangeName = ExchangeName.ToUpper();
            QueueName = queue.ToUpper();
        }

        /// <summary>
        /// Sets up the data adatpor for translating the outgoing data into 
        /// an object of another type.
        /// </summary>
        /// <param name="f"></param>
        public void SetDataAdaptor<U>(Func<T, U> f) where U : class
        {
            OutDataAdpator = f;
        }

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
                // Otherwise, we use the default exchange ""
                if (!string.IsNullOrEmpty(ExchangeName))
                {
                    that.Channel.ExchangeDeclare(ExchangeName, "direct");
                }
            });
        }


        public virtual bool SendMessage(T data)
        {
            return PublishSafely((channelDecorator) =>
            {

                EnsureExchangeDeclared(channelDecorator);

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                var props = BuildChannelProperties(channelDecorator);

                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: QueueName,
                                     basicProperties: props,
                                     body: bytes);
                // It is the consumer's responsibility to declare queue
            });
        }
    }
}
