using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class SubscriptionService<TIn, TInter> :
        AbstractConnection, ISubscriptionService<TIn, TInter>
        where TIn : class
        where TInter : class
    {
        public string ExchangeName { get; private set; }

        protected Func<object, Tuple<TIn, TInter>> InDataAdaptor;
        protected Func<TIn, TInter, int> InDataHandler;
        protected Func<Exception, Tuple<bool, bool>> ExceptionHandler;

        protected SubscriptionChannelFeature CallbackFeature;
        protected ChannelDecorator EffectiveChannelDecorator;

        public string SubscriptionQueueName { get; protected set; }

        public SubscriptionService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            IDictionary<string, object> settings)
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            ExchangeName = exchange;
            InDataAdaptor = x => Tuple.Create<TIn, TInter>(x as TIn, null);

            CallbackFeature = new SubscriptionChannelFeature();
            // Set up for exceptions:
            CallbackFeature.CallbackConsumer.Shutdown += (model, args) =>
            {
                // todo: any
            };

            CallbackFeature.DataHandler = (message) =>
            {
                try
                {
                    // todo: Deserialize into an object
                    var body = message.Body;
                    // todo: 
                    var payload = Runtime.Serialization.ByteConvertor.ByteArrayToObject(body.ToArray());
                    var data = InDataAdaptor(payload);
                    var code = InDataHandler(data.Item1, data.Item2);
                    PostHandling(code, data.Item1, message);
                }
                catch (Exception e)
                {
                    if (ExceptionHandler != null)
                    {
                        var ret = ExceptionHandler(e);
                        if (ret.Item1)
                        {
                            AckMessage(new BasicAckEventArgs
                            {
                                DeliveryTag = message.DeliveryTag,
                                Multiple = false
                            });
                        }
                        else
                        {
                            NAckMessage(new BasicNackEventArgs
                            {
                                DeliveryTag = message.DeliveryTag,
                                Multiple = false
                            }, ret.Item2);
                        }
                    }

                    // By default, reject but do not reenque
                    NAckMessage(new BasicNackEventArgs
                    {
                        DeliveryTag = message.DeliveryTag,
                        Multiple = false
                    }, false);
                }
            };
        }

        protected abstract void BuildOrBindQueue(ChannelDecorator channelDecorator);

        public void Subscribe(Func<object, Tuple<TIn, TInter>> adaptor = null, Func<TIn, TInter, int> handler = null)
        {
            if (adaptor != null)
            {
                InDataAdaptor = adaptor;
            }

            if (handler != null)
            {
                InDataHandler = handler;
            }

            SubscribeInner();
        }

        private void SubscribeInner()
        {
            if (!ReconnectionState.CanReconnect)
            {
                // No more try
                return;
            }

            try
            {
                EffectiveChannelDecorator = ChannelPool.Get(ChannelName, ConnectionName);
                CallbackFeature.SetupCallback(EffectiveChannelDecorator);

                BuildOrBindQueue(EffectiveChannelDecorator);

                EffectiveChannelDecorator.Channel.BasicConsume(queue: SubscriptionQueueName,
                    autoAck: (bool)Settings["autoAck"],
                    consumer: CallbackFeature.CallbackConsumer);

                ReconnectionState.Reset();
            }
            catch (UnexpectedConnectionDecoratorException)
            {
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
                SubscribeInner();
            }
            catch (UnexpectedChannelDecoratorException)
            {
                ChannelPool.Clear();

                ReconnectionState.BumpCounter();
                SubscribeInner();
            }
            catch (AlreadyClosedException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
                SubscribeInner();
            }
            catch (BrokerUnreachableException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
                SubscribeInner();
            }
            catch (ConnectFailureException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
                SubscribeInner();
            }
            catch (Exception)
            {
                ReconnectionState.BumpCounter();
                SubscribeInner();
            }

        }

        protected void AckMessage(BasicAckEventArgs message)
        {
            EffectiveChannelDecorator?.Channel.BasicAck(deliveryTag: message.DeliveryTag, multiple: message.Multiple);
        }

        protected void NAckMessage(BasicNackEventArgs message, bool requeue)
        {
            EffectiveChannelDecorator?.Channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: message.Multiple, requeue: requeue);
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
            InDataAdaptor = adaptor;
        }

        /// <summary>
        /// Builds with a callback to process the generated data from the incoming message.
        /// </summary>
        /// <param name="handler">Functon to process the generated data from the incoming message.</param>
        public void SetDataHandler(Func<TIn, TInter, int> handler)
        {
            InDataHandler = handler;
        }

        public void SetExceptionHandler(Func<Exception, Tuple<bool, bool>> handler)
        {
            ExceptionHandler = handler;
        }

        public bool IsOperating => EffectiveChannelDecorator != null ? EffectiveChannelDecorator.Channel.IsOpen : false;
    }
}
