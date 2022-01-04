using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class ChannelDecorator : IDisposable
    {
        private readonly object _locker = new object();

        public IModel Channel { get; private set; }

        public string ConnectionName { get; private set; }

        public string Name { get; private set; }

        // todo: Volatile???
        public bool IsOpen { get; private set; }

        // Is it possible that one connection is disposed???
        public bool IsDisposed { get; private set; }

        public ChannelDecorator(string name, IModel channel, string connectionName)
        {
            Channel = channel;
            ConnectionName = connectionName;
            Name = name;
            IsOpen = true;
        }

        // Note that this method will not handle exceptions.
        // It only ensure that we share the channel; excpetions would be 
        // thrown up.
        public void PublishSafely(Action<IModel> action)
        {
            lock(_locker)
            {
                action.Invoke(Channel);
            }
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            try
            {
                Channel?.Close();
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
                Channel?.Dispose();
                IsDisposed = true;
            }
        }
    }
}
