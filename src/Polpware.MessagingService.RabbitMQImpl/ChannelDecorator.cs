using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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

        public IChannelCallbackFeature RpcChannelFeature { get; set; }

        // todo: Do we need to consider thread-safe (parallel)?
        // todo: Maybe not, because a channel can only used in one place at a time.
        // Properties
        private IBasicProperties _properties = null;
        private bool _exchangeDeclared;

        private List<string> _boundQueues;

        public ChannelDecorator(string name, IModel channel, string connectionName)
        {
            Channel = channel;
            ConnectionName = connectionName;
            Name = name;
            IsOpen = true;
            _boundQueues = new List<string>();
        }

        // Note that this method will not handle exceptions.
        // It only ensure that we share the channel; excpetions would be 
        // thrown up.
        public void PublishSafely(Action<ChannelDecorator> action)
        {
            lock(_locker)
            {
                action.Invoke(this);
            }
        }

        public IBasicProperties GetOrCreateProperties(Func<ChannelDecorator, IBasicProperties> func)
        {
            if (_properties == null)
            {
                _properties = func(this);
            }
            return _properties;
        }

        public void EnsureExchangDeclared(Action<ChannelDecorator> action)
        {
            if (!_exchangeDeclared)
            {
                action.Invoke(this);
                _exchangeDeclared = true;
            }
        }

        public void EnsureQueueBinded(string queue, Action<ChannelDecorator> action)
        {
            // Normalize 
            queue = queue.ToUpper();
            if (!_boundQueues.Contains(queue))
            {
                action.Invoke(this);
                _boundQueues.Add(queue);
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
                RpcChannelFeature?.TearOffCallback(this);
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
