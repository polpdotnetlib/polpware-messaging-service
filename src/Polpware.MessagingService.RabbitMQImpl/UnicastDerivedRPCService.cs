using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class UnicastDerivedRPCService<TCall, TReturn> : UnicastService<TCall>, IRPCLike<TCall, TReturn> 
        where TCall : class
        where TReturn: class
    {
        private RPCChannelFeature<TReturn> _RPCChannelFeature;

        public UnicastDerivedRPCService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            string queue, 
            IDictionary<string, object> settings, 
            string replyQueue) 
            : base(connectionPool, channelPool, connectionName, channelName, exchange, queue, settings)
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
            var p = channelDecorator.GetOrCreateProperties((that) =>
            {
                var properties = that.Channel.CreateBasicProperties();
                properties.Persistent = (bool)Settings["persistent"];
                properties.CorrelationId = _RPCChannelFeature.CorrelationId;
                properties.ReplyTo = _RPCChannelFeature.CallbackQueueName;

                return properties;
            });

            return p;
        }


        public void Call(TCall data, params object[] options)
        {
            SendMessage(data);

        }

        public override bool SendMessage(TCall data)
        {

            return PublishSafely((channelDecorator) =>
            {

                EnsureExchangeDeclared(channelDecorator);
                _RPCChannelFeature.SetupCallback(channelDecorator);

                var x = OutDataAdpator(data);
                var bytes = Runtime.Serialization.ByteConvertor.ObjectToByteArray(x);

                // Set up listener
                channelDecorator.Channel.BasicConsume(_RPCChannelFeature.CallbackConsumer,
                    queue: _RPCChannelFeature.CallbackQueueName,
                    autoAck: true);

                // Must call this after invoking SetupCallback
                var props = BuildChannelProperties(channelDecorator);
                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                                                  routingKey: QueueName,
                                                  basicProperties: props,
                                                  body: bytes);
            });
        }
    }
}
