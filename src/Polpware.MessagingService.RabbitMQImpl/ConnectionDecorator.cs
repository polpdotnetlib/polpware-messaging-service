using RabbitMQ.Client;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class ConnectionDecorator: IDisposable
    {
        public string Name { protected set; get; }
        public IConnection Connection { protected set; get; }

        // Is it possible that one connection is disposed???
        public bool IsDisposed { get; private set; }

        public ConnectionDecorator(string name, IConnection conn)
        {
            Name = name;
            Connection = conn;
        }

        // Is Dispose reentrant?
        public void Dispose()
        {
            IsDisposed = true;
            Connection?.Dispose();
        }
    }
}
