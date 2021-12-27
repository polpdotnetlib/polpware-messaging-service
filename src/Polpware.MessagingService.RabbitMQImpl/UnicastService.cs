using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class UnicastService<T> : AbstractConnection, IUnicastService<T> where T : class
    {
        protected IBasicProperties _props;

        protected Func<T, object> _dataAdpator;

        public UnicastService(ConnectionFactory connectionFactory, string queue, IDictionary<string, object> settings) 
            : base(connectionFactory, settings)
        {
            _dataAdpator = id => id;

            existingConnection.QueueName = queue;
            
            Init();

            PrepareProperties();
        }

        /// <summary>
        /// Sets up the data adatpor for translating the outgoing data into 
        /// an object of another type.
        /// </summary>
        /// <param name="f"></param>
        public void SetDataAdaptor<U>(Func<T, U> f) where U : class
        {
            _dataAdpator = f;
        }

        /// <summary>
        /// Some initialization code before preparing properties.
        /// </summary>
        protected virtual void Init() { }

        protected virtual void PrepareProperties()
        {
            _props = existingConnection.Channel.CreateBasicProperties();
            _props.Persistent = (bool)_settings["persistent"];
        }

        public virtual bool SendMessage(T data)
        {
            try
            {
                existingConnection.Channel.QueueDeclare(queue: existingConnection.QueueName,
                    durable: (bool)_settings["durable"],
                    exclusive: (bool)_settings["exclusive"],
                    autoDelete: (bool)_settings["autoDelete"],
                    arguments: null);

                var x = _dataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                existingConnection.Channel.BasicPublish(exchange: "",
                                     routingKey: existingConnection.QueueName,
                                     basicProperties: _props,
                                     body: bytes);
                return true;
            }
            catch (Exception e)
            {
                if (ReBuildConnection())
                {
                    return this.SendMessage(data);
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
