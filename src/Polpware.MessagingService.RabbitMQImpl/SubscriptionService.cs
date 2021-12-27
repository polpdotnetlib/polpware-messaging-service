using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class SubscriptionService<TIn, TInter> :
        AbstractConnection, ISubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter: class
    {
        protected Func<object, Tuple<TIn, TInter>> _inDataAdaptor;
        protected Func<TIn, TInter, int> _inDataHandler;
        protected Func<Exception, Tuple<bool, bool>> _exceptionHandler;

        public SubscriptionService(ConnectionFactory connectionFactory, IDictionary<string, object> settings) 
            : base(connectionFactory, settings)
        {
            _inDataAdaptor = x => Tuple.Create<TIn, TInter>(x as TIn, null);
        }

        protected abstract void BuildOrBindQueue();

        public void Subscribe(Func<object, Tuple<TIn, TInter>> adaptor = null, Func<TIn, TInter, int> handler = null)
        {
            if (adaptor != null)
            {
                _inDataAdaptor = adaptor;
            }

            if (handler != null)
            {
                _inDataHandler = handler;
            }

            this.BuildOrBindQueue();

            try
            {
                this.SubscribeInner();
            }
            catch (Exception e)
            {
                if (this.ReBuildConnection())
                {
                    this.SubscribeInner();
                }

                throw new MessagingServiceException(e, 0, "");
            }
        }

        private void SubscribeInner()
        {
            var consumer = new EventingBasicConsumer(existingConnection.Channel);
            consumer.Received += (model, message) =>
            {
                try
                {
                    // todo: Deserialize into an object
                    var body = message.Body;
                    // todo: 
                    var payload = Runtime.Serialization.ByteConvertor.ByteArrayToObject(body);
                    var data = _inDataAdaptor(payload);
                    var code = _inDataHandler(data.Item1, data.Item2);
                    PostHandling(code, data.Item1, message);
                } catch (Exception e)
                {
                    if (_exceptionHandler != null)
                    {
                        var ret = _exceptionHandler(e);
                        if (ret.Item1)
                        {
                            this.AckMessage(new BasicAckEventArgs
                            {
                                DeliveryTag = message.DeliveryTag,
                                Multiple = false
                            });
                        }
                        else
                        {
                            this.NAckMessage(new BasicNackEventArgs
                            {
                                DeliveryTag = message.DeliveryTag,
                                Multiple = false
                            }, ret.Item2);
                        }
                    }

                    // By default, reject but do not reenque
                    this.NAckMessage(new BasicNackEventArgs
                    {
                        DeliveryTag = message.DeliveryTag,
                        Multiple = false
                    }, false);
                }
            };

            existingConnection.Channel.BasicConsume(queue: existingConnection.QueueName, 
                autoAck: (bool)_settings["autoAck"],
                consumer:consumer);
        }

        protected void AckMessage(BasicAckEventArgs message)
        {
            existingConnection.Channel.BasicAck(deliveryTag: message.DeliveryTag, multiple: message.Multiple);
        }

        protected void NAckMessage(BasicNackEventArgs message, bool requeue)
        {
            existingConnection.Channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: message.Multiple, requeue: requeue);
        }

        /// <summary>
        /// Defines whether we need to ack a message or not.
        /// </summary>
        /// <param name="code">Processing result</param>
        /// <param name="data">Data</param>
        /// <param name="evt"></param>
        protected abstract void PostHandling(int code, TIn data, BasicDeliverEventArgs evt);

        /// <summary>
        /// Builds with an adaptor to process the incoming message.
        /// </summary>
        /// <param name="adaptor">Function to translate the incoming message.</param>
        public void SetDataAdaptor(Func<object, Tuple<TIn, TInter>> adaptor)
        {
            _inDataAdaptor = adaptor;
        }

        /// <summary>
        /// Builds with a callback to process the generated data from the incoming message.
        /// </summary>
        /// <param name="handler">Functon to process the generated data from the incoming message.</param>
        public void SetDataHandler(Func<TIn, TInter, int> handler)
        {
            _inDataHandler = handler;
        }

        public void SetExceptionHandler(Func<Exception, Tuple<bool, bool>> handler)
        {
            _exceptionHandler = handler;
        }

        public bool IsOperating => this.existingConnection.Channel.IsOpen;
    }
}
