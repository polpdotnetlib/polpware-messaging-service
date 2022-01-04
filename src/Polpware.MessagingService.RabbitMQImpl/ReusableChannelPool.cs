using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class ReusableChannelPool : IChannelPool
    {
        public IConnectionPool ConnectionPool { get; }

        protected ConcurrentDictionary<string, ChannelDecorator> Channels { get; }

        public ReusableChannelPool(IConnectionPool connectionPool)
        {
            ConnectionPool = connectionPool;
            Channels = new ConcurrentDictionary<string, ChannelDecorator>();
        }

        public ChannelDecorator Get(string channelName = null, string connectionName = null)
        {
            var conn = ConnectionPool.Get(connectionName);
            channelName = channelName ?? "";

            var uniqueKey = $"{conn.Name}-{channelName}";

            var item = Channels.GetOrAdd(
                uniqueKey,
                _ => conn.Connection.CreateModel().Map2Decorator(channelName, conn.Name)
            );

            if (item.IsDisposed || !item.IsOpen)
            {
                throw new UnexpectedChannelDecoratorException();
            }

            return item;
        }

        public void Remove(string channelName, string connectionName)
        {
            var uniqueKey = $"{connectionName}-{channelName}";
            Channels.TryRemove(connectionName, out ChannelDecorator entry);
            if (entry != null)
            {
                // Should not throw any exception
                entry.Close();
                entry.Dispose();
            }

        }

        public void Clear()
        {
            foreach (var entry in Channels.Values)
            {
                entry.Close();
                entry.Dispose();
            }

            Channels.Clear();
        }
    }
}
