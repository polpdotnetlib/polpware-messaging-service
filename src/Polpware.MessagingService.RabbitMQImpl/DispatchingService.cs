using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class DispatchingService<TOut> : AbstractConnection, IDispatchingService<TOut> where TOut : class
    {
        protected readonly string _exchange;
        protected IBasicProperties _props;

        protected Func<TOut, object> _outDataAdpator;

        public DispatchingService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings)
            : base(connectionFactory, settings)
        {
            _outDataAdpator = x => x;
            _exchange = exchange;

            Init();

            PrepareProperties();
        }

        public void SetDataAdaptor<U>(Func<TOut, U> f) where U : class
        {
            _outDataAdpator = f;
        }

        /// <summary>
        /// Some initialization code before preparing properties.
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// Properties to be used for sending out messages.
        /// </summary>
        protected virtual void PrepareProperties()
        {
            _props = existingConnection.Channel.CreateBasicProperties();
            _props.Persistent = (bool)_settings["persistent"];
        }

        public virtual bool DispatchMessage(TOut data, string routingKey)
        {
            try
            {

                existingConnection.Channel.ExchangeDeclare(_exchange, "direct");

                var x = _outDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                existingConnection.Channel.BasicPublish(exchange: _exchange,
                                     routingKey: routingKey,
                                     basicProperties: _props,
                                     body: bytes);
                // Ok
                return true;
            }
            catch (Exception e)
            {
                // todo: Handle exception

                if (ReBuildConnection())
                {
                    return this.DispatchMessage(data, routingKey);
                }

                throw new MessagingServiceException(e, 0, "");
            }
        }

        public override bool ReBuildConnection()
        {
            var f = base.ReBuildConnection();
            PrepareProperties();
            return f;
        }
    }
}
