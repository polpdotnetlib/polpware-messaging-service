using RabbitMQ.Client;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public static class Extensions
    {
        public static ConnectionDecorator Map2Decorator(this IConnection conn, string name)
        {
            return new ConnectionDecorator(name, conn);
        }
    }
}
