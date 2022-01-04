using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    /// <summary>
    /// A service for dispatching messages out, while expecting replies to those messages. 
    /// Thus, it resembles a RPC-like messaging pattern, underpinned by the dispatching service. 
    /// </summary>
    /// <typeparam name="TCall">Message to be sent out, while the replied message is an Object.</typeparam>
    public class DispatchingDerivedRPCService<TCall, TReturn> : DispatchingService<TCall>, IRPCLike<TCall, TReturn> 
        where TCall : class
        where TReturn: class
    {
        private RPCInitHelper<TReturn> _RPCInitHelper;
        private readonly string _replyQueue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionFactory">RabbitMQ-specific connection factory</param>
        /// <param name="exchange">Exchange name</param>
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues. 
        /// Amongest them, durable, persistent, exclusive, and autoDelete are used to define the properties of queue,
        /// while autoAck is to define the properties of the consumer (event handler).</param>
        /// <param name="replyQueue">Specific queue for replying messages. Most time leave it empty so that an anonymous queue will be computed internally.</param>
        public DispatchingDerivedRPCService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            IDictionary<string, object> settings,
            string replyQueue) : 
            base(connectionPool, channelPool, connectionName, channelName, exchange, settings)
        {
            _replyQueue = replyQueue;
        }

        public void SetReturnAdaptor(Func<object, TReturn> func)
        {
            _RPCInitHelper.ReturnAdaptor = func;
        }

        public void SetReturnHandler(Action<TReturn> action)
        {
            _RPCInitHelper.ReturnHandler = action;
        }


        protected override IBasicProperties BuildChannelProperties(ChannelDecorator channelDecorator)
        {
            var p = channelDecorator.GetOrCreateProperties((that) =>
            {
                var properties = that.Channel.CreateBasicProperties();
                properties.Persistent = (bool)Settings["persistent"];
                properties.CorrelationId = _RPCInitHelper.CorrelationId;
                properties.ReplyTo = _RPCInitHelper.CallbackQueueName;

                return properties;
            });

            return p;
        }

        public void Call(TCall data, params object[] options)
        {
            var routingKey = options.Length > 0 ? options[0].ToString() : "";

            DispatchMessage(data, routingKey);

        }

        public override bool DispatchMessage(TCall data, string routingKey)
        {

            return PublishSafely((channelDecorator) =>
            {

                EnsureExchangeDeclared(channelDecorator);
                _RPCInitHelper.SetupCallback(channelDecorator);

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);


                // Set up listener
                channelDecorator.Channel.BasicConsume(_RPCInitHelper.CallbackConsumer,
                    queue: _RPCInitHelper.CallbackQueueName,
                    autoAck: true);

                // Must call this after _RPCInitHelper
                var props = BuildChannelProperties(channelDecorator);
                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: Props,
                                     body: bytes);
            });
        }
    }
}
