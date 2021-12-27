using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class BroadcastService<TOut> : AbstractConnection, IBroadcastService<TOut> where TOut : class
    {
        private readonly string _exchange;

        protected Func<TOut, object> _outDataAdpator;

        public BroadcastService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings) 
            : base(connectionFactory, settings)
        {
            _outDataAdpator = x => x;

            _exchange = exchange;
        }

        public void SetDataAdaptor<U>(Func<TOut, U> f) where U : class
        {
            _outDataAdpator = f;
        }

        public bool BroadcastMessage(TOut data)
        {
            try {

                existingConnection.Channel.ExchangeDeclare(_exchange, "fanout");

                var properties = existingConnection.Channel.CreateBasicProperties();
                properties.Persistent = (bool)_settings["persistent"];

                var x = _outDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                existingConnection.Channel.BasicPublish(exchange: _exchange,
                                     routingKey: "",
                                     basicProperties: properties,
                                     body: bytes);
                // Ok
                return true;
            }
            catch (Exception e)
            {
                // todo: Handle exception

                if (ReBuildConnection())
                {
                    return this.BroadcastMessage(data);
                }

                throw new MessagingServiceException(e, 0, "");
            }
        }
    }
}
