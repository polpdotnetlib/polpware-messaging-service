using System;
using RabbitMQ.Client;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public interface IChannelCallbackFeature
    {
        void SetupCallback(ChannelDecorator channelDecorator);

        void TearOffCallback(ChannelDecorator channelDecorator);
    }
}
