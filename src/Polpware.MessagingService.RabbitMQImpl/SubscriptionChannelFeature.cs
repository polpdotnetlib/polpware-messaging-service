using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class SubscriptionChannelFeature: IChannelCallbackFeature
    {
        public EventingBasicConsumer CallbackConsumer { get; private set; }

        public Action<BasicDeliverEventArgs> DataHandler { get; set; }

        public Action<ShutdownEventArgs> ShutdownHandler { get; set; }

        private EventHandler<ShutdownEventArgs> _shutdownDelegate;
        private EventHandler<BasicDeliverEventArgs> _callbackDelegate;
        private bool _hooked;

        public SubscriptionChannelFeature()
        {
            _callbackDelegate = (model, ea) =>
            {
                DataHandler?.Invoke(ea);
            };

            _shutdownDelegate = (model, ea) =>
            {
                ShutdownHandler?.Invoke(ea);
            };
        }

        public void SetupCallback(ChannelDecorator channelDecorator)
        {
            if (!_hooked)
            {
                // Create our consumer
                CallbackConsumer = new EventingBasicConsumer(channelDecorator.Channel);

                CallbackConsumer.Shutdown += _shutdownDelegate;
                CallbackConsumer.Received += _callbackDelegate;
                channelDecorator.RpcChannelFeature = this;
                _hooked = true;
            }
        }

        public void TearOffCallback(ChannelDecorator channelDecorator)
        {
            if (_hooked)
            {
                CallbackConsumer.Received -= _callbackDelegate;
                channelDecorator.RpcChannelFeature = null;
                _hooked = false;
            }
        }
    }
}
