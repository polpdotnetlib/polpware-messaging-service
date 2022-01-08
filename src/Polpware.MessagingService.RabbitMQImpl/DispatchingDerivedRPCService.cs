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
    /// <typeparam name="TReturn">Return type</typeparam>
    public class DispatchingDerivedRPCService<TCall, TReturn> : DispatchingService<TCall>, IRPCLike<TCall, TReturn> 
        where TCall : class
        where TReturn: class
    {
        private RPCChannelFeature<TReturn> _RPCChannelFeature;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionPool">Connection pool</param>
        /// <param name="channelPool">Channel pool</param>
        /// <param name="connectionName">Connection name</param>
        /// <param name="channelName">Channel name</param>
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
            _RPCChannelFeature = new RPCChannelFeature<TReturn>(replyQueue);
        }

        public void SetReturnAdaptor(Func<object, TReturn> func)
        {
            _RPCChannelFeature.ReturnAdaptor = func;
        }

        public void SetReturnHandler(Action<TReturn> action)
        {
            _RPCChannelFeature.ReturnHandler = action;
        }

        protected override IBasicProperties BuildChannelProperties(ChannelDecorator channelDecorator)
        {
            var pp = channelDecorator.GetOrCreateProperties((that) =>
            {
                var properties = that.Channel.CreateBasicProperties();
                properties.Persistent = (bool)Settings["persistent"];
                properties.CorrelationId = _RPCChannelFeature.CorrelationId;
                properties.ReplyTo = _RPCChannelFeature.CallbackQueueName;

                return properties;
            });

            return pp;
        }

        public void Call(TCall data, params object[] options)
        {
            var routingKey = options.Length > 0 ? options[0].ToString() : "";

            DispatchMessage(data, routingKey);

        }

        public override bool DispatchMessage(TCall data, string routingKey)
        {
            routingKey = routingKey.ToUpper();

            return PublishSafely((channelDecorator) =>
            {

                EnsureExchangeDeclared(channelDecorator);

                // Set up callback queue
                channelDecorator.EnsureQueueBinded(_RPCChannelFeature.CallbackQueueName, (that) =>
                {

                    that.Channel.QueueDeclare(_RPCChannelFeature.CallbackQueueName,
                        durable: (bool)Settings["durable"],
                        exclusive: (bool)Settings["exclusive"],
                        autoDelete: (bool)Settings["autoDelete"]);

                    // Routing key must agree with callback queue name.
                    that.Channel.QueueBind(queue: _RPCChannelFeature.CallbackQueueName,
                             exchange: ExchangeName,
                             routingKey: _RPCChannelFeature.CallbackQueueName);
                });
                _RPCChannelFeature.SetupCallback(channelDecorator);

                // Set up listener
                // Must call this after invoking SetupCallback
                channelDecorator.Channel.BasicConsume(_RPCChannelFeature.CallbackConsumer,
                    queue: _RPCChannelFeature.CallbackQueueName,
                    autoAck: true);

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                var gg = BuildChannelProperties(channelDecorator);

                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: routingKey,
                                     basicProperties: gg,
                                     body: bytes);
            });
        }
    }
}
