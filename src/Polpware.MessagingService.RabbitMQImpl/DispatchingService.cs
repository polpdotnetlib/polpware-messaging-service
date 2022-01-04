using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class DispatchingService<TOut> : AbstractConnection, IDispatchingService<TOut> where TOut : class
    {
        protected readonly string ExchangeName;
        protected IBasicProperties Props;

        protected Func<TOut, object> OutDataAdpator;

        public DispatchingService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings)
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            OutDataAdpator = x => x;

            ExchangeName = exchange;

            Init();

            PrepareProperties();
        }

        public void SetDataAdaptor<U>(Func<TOut, U> f) where U : class
        {
            OutDataAdpator = f;
        }

        /// <summary>
        /// Some initialization code before preparing properties.
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// Properties to be used for sending out messages.
        /// </summary>
        protected virtual void PrepareProperties(IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = (bool)Settings["persistent"];
        }

        public virtual bool DispatchMessage(TOut data, string routingKey)
        {

            return PublishSafely((channel) => {
                channel.ExchangeDeclare(ExchangeName, "direct");
                PrepareProperties(channel);

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: Props,
                                     body: bytes);
            });
        }

        public override bool ReBuildConnection()
        {
            var f = base.ReBuildConnection();
            PrepareProperties();
            return f;
        }
    }
}
