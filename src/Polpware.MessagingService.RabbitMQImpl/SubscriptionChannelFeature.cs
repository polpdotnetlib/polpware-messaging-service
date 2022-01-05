using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class SubscriptionChannelFeature: IChannelCallbackFeature
    {
        public string CallbackQueueName { get; private set; }
        public EventingBasicConsumer CallbackConsumer { get; private set; }

        public Action<BasicDeliverEventArgs> DataHandler { get; set; }
        public string CorrelationId { get; }

        private EventHandler<BasicDeliverEventArgs> _callbackDelegate;
        private bool _hooked;

        public SubscriptionChannelFeature()
        {
            CallbackQueueName = "";
            CorrelationId = Guid.NewGuid().ToString();

            _callbackDelegate = (model, ea) =>
            {
                DataHandler(ea);
            };
        }

        public void SetupCallback(ChannelDecorator channelDecorator)
        {
            if (!_hooked)
            {
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
