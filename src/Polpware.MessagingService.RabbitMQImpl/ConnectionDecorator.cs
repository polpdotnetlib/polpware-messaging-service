using RabbitMQ.Client;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class ConnectionDecorator: IDisposable
    {
        public string Name { protected set; get; }
        public IConnection Connection { protected set; get; }

        // todo: Volatile???
        public bool IsOpen { get; private set; }
        // Is it possible that one connection is disposed???
        public bool IsDisposed { get; private set; }

        public ConnectionDecorator(string name, IConnection conn)
        {
            Name = name;
            Connection = conn;
            IsOpen = true;
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            try
            {
                Connection?.Close();
            } 
            catch (Exception e)
            {
                // Catch all exceptions
            }
            finally
            {
                IsOpen = false;
            }
        }

        // Is Dispose reentrant?
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Connection?.Dispose();
                IsDisposed = true;
            }
        }
    }
}
