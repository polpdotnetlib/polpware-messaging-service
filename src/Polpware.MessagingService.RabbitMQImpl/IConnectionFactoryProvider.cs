using RabbitMQ.Client;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public interface IConnectionFactoryProvider
    {
        string DefaultConnectionName { get; }
        ConnectionFactory Build();
    }
}
