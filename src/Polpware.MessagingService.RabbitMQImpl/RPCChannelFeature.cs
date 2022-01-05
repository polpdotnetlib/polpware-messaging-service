﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class RPCChannelFeature<TReturn>: IChannelCallbackFeature where TReturn : class
    {
        public string CallbackQueueName { get; private set; }
        public EventingBasicConsumer CallbackConsumer { get; private set; }

        public Func<object, TReturn> ReturnAdaptor { get; set; }
        public Action<TReturn> ReturnHandler { get; set; }
        public string CorrelationId { get; }

        private readonly bool _isAnonymousReplyQueue;
        private EventHandler<BasicDeliverEventArgs> _callbackDelegate;
        private bool _hooked;

        public RPCChannelFeature(string replyQueue)
        {
            CallbackQueueName = replyQueue;
            _isAnonymousReplyQueue = string.IsNullOrEmpty(replyQueue);
            CorrelationId = Guid.NewGuid().ToString();

            ReturnAdaptor = x => x as TReturn;

            _callbackDelegate = (model, ea) =>
            {
                // todo: Deserialize into an object
                var body = ea.Body;
                // filter messages
                if (ea.BasicProperties.CorrelationId == CorrelationId)
                {
                    // todo: 
                    var payload = Runtime.Serialization.ByteConvertor.ByteArrayToObject(body.ToArray());

                    var data = ReturnAdaptor(payload);
                    ReturnHandler?.Invoke(data);
                }

                // Auto Ack anyway
                // existingConnection.Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };
        }

        public void SetupCallback(ChannelDecorator channelDecorator)
        {
            if (!_hooked)
            {
                if (_isAnonymousReplyQueue)
                {
                    // Create one
                    CallbackQueueName = channelDecorator.Channel.QueueDeclare().QueueName;
                }

                // Create our consumer
                CallbackConsumer = new EventingBasicConsumer(channelDecorator.Channel);

                CallbackConsumer.Received += _callbackDelegate;
                channelDecorator.RpcChannelFeature = this;
                _hooked = true;
            }
        }

        public void TearOffCallback()
        {
            if (_hooked)
            {
                CallbackConsumer.Received -= _callbackDelegate;
                _hooked = false;
            }
        }
    }
}
