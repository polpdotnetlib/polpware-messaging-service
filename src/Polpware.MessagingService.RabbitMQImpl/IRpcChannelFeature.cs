using System;
using RabbitMQ.Client;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public interface IRPCChannelFeature
    {
        string CallbackQueueName { get; }
        string CorrelationId { get; }

        void SetupCallback(ChannelDecorator channelDecorator);

        void TearOffCallback();
    }
}
