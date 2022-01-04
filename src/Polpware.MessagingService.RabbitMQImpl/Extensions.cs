using RabbitMQ.Client;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public static class Extensions
    {
        public static ConnectionDecorator Map2Decorator(this IConnection conn, string name)
        {
            return new ConnectionDecorator(name, conn);
        }

        public static ChannelDecorator Map2Decorator(this IModel channel, string name, string connName)
        {
            return new ChannelDecorator(name, channel, connName);
        }
    }
}
