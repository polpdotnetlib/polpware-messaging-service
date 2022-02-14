using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;

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

        public string SubscriptionQueueName { get; protected set; }

        public SubscriptionService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            IDictionary<string, object> settings)
            : base(connectionPool, channelPool, connectionName, channelName, settings)
        {
            // Normlize exchange name
            SetExchangeName(exchange);
            InDataAdaptor = x => Tuple.Create<TIn, TInter>(x as TIn, null);   
        }

        public SubscriptionService(IConnectionPool connectionPool,
            IChannelPool channelPool)
            : base(connectionPool, channelPool)
        {
            InDataAdaptor = x => Tuple.Create<TIn, TInter>(x as TIn, null);
        }

        protected void SetExchangeName(string exchange)
        {
            ExchangeName = exchange.ToUpper();
        }

        protected abstract void BuildOrBindQueue(ChannelDecorator channelDecorator);

        // Default implementation for the subscription 
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

            // cannot run a separate thread
            // todo: why?

            SubscribeInner();
        }

        private void SubscribeInner()
        {

            if (!ReconnectionState.CanReconnect)
            {
                // No more try
                return;
            }

            // wait for some time
            if (ReconnectionState.ReconnectionCounter > 0)
            {
                // todo: Is this good?
                Thread.Sleep(1000 * 60 * ReconnectionState.ReconnectionCounter * ReconnectionState.ReconnectionCounter);
            }

            try
            {
                var channelDecorator = ChannelPool.Get(ChannelName, ConnectionName);
                var callbackFeature = new SubscriptionChannelFeature();
                // Set up some handlers
                callbackFeature.DataHandler = (message) =>
                {
                    try
                    {
                        // todo: Deserialize into an object
                        var body = message.Body;
                        // todo: 
                        var payload = Runtime.Serialization.ByteConvertor.ByteArrayToObject(body.ToArray());
                        var data = InDataAdaptor(payload);
                        var code = InDataHandler(data.Item1, data.Item2);
                        PostHandling(channelDecorator, code, data.Item1, message);
                    }
                    catch (Exception e)
                    {
                        if (ExceptionHandler != null)
                        {
                            var ret = ExceptionHandler(e);
                            if (ret.Item1)
                            {
                                AckMessage(channelDecorator, new BasicAckEventArgs
                                {
                                    DeliveryTag = message.DeliveryTag,
                                    Multiple = false
                                });
                            }
                            else
                            {
                                NAckMessage(channelDecorator, new BasicNackEventArgs
                                {
                                    DeliveryTag = message.DeliveryTag,
                                    Multiple = false
                                }, ret.Item2);
                            }
                        }
                        else
                        {
                            // By default, reject but do not reenque
                            NAckMessage(channelDecorator, new BasicNackEventArgs
                            {
                                DeliveryTag = message.DeliveryTag,
                                Multiple = false
                            }, false);
                        }
                    }
                };
                callbackFeature.ShutdownHandler = (message) =>
                {
                    // cannot restart
                    if (message.ReplyCode != 200)
                    {
                        ReconnectionState.BumpCounter();
                        this.SubscribeInner();
                    }
                };

                callbackFeature.SetupCallback(channelDecorator);

                EnsureExchangeDeclared(channelDecorator);
                BuildOrBindQueue(channelDecorator);

                channelDecorator.Channel.BasicConsume(queue: SubscriptionQueueName,
                    autoAck: (bool)Settings["autoAck"],
                    consumer: callbackFeature.CallbackConsumer);

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
            catch (Exception e)
            {
                ReconnectionState.BumpCounter();
                SubscribeInner();
            }

        }

        protected void AckMessage(ChannelDecorator channelDecorator, BasicAckEventArgs message)
        {
            channelDecorator.Channel.BasicAck(deliveryTag: message.DeliveryTag, multiple: message.Multiple);
        }

        protected void NAckMessage(ChannelDecorator channelDecorator, BasicNackEventArgs message, bool requeue)
        {
            channelDecorator.Channel.BasicNack(deliveryTag: message.DeliveryTag, multiple: message.Multiple, requeue: requeue);
        }

        /// <summary>
        /// Defines whether we need to ack a message or not.
        /// </summary>
        /// <param name="channelDecorator">Channel decorator</param>
        /// <param name="code">Processing result</param>
        /// <param name="data">Data</param>
        /// <param name="evt"></param>
        protected abstract void PostHandling(ChannelDecorator channelDecorator, int code, TIn data, BasicDeliverEventArgs evt);

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

        public bool IsOperating => ReconnectionState.CanReconnect;
    }
}
