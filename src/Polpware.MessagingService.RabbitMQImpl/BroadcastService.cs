using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class BroadcastService<TOut> : AbstractConnection, IBroadcastService<TOut> where TOut : class
    {
        protected string ExchangeName { get; }

        protected Func<TOut, object> OutDataAdpator;

        public BroadcastService(IConnectionPool connectionPool,
            IChannelPool channelPool, 
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings) 
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            OutDataAdpator = x => x;

            ExchangeName = exchange;
        }

        public void SetDataAdaptor<U>(Func<TOut, U> f) where U : class
        {
            OutDataAdpator = f;
        }

        public bool BroadcastMessage(TOut data)
        {
            return PublishSafely((channel) => {
                channel.ExchangeDeclare(ExchangeName, "fanout");
                var properties = channel.CreateBasicProperties();
                properties.Persistent = (bool)Settings["persistent"];

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: "",
                                     basicProperties: properties,
                                     body: bytes);
            });

        }
    }
}
