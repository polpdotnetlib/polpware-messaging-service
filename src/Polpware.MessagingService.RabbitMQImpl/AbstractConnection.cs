using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class AbstractConnection
    {
        protected IConnectionPool ConnectionPool { get; }
        protected IChannelPool ChannelPool { get; set; }

        protected string ConnectionName { get; }
        protected string ChannelName { get; }

        protected readonly IDictionary<string, object> Settings;

        protected readonly ReconnectionTracker ReconnectionState;

        public AbstractConnection(IConnectionPool connectionPool,
            IChannelPool channelPool, 
            string connectionName,
            string channelName,
            IDictionary<string, object> settings)
        {
            ConnectionPool = connectionPool;
            ConnectionName = connectionName;
            ChannelPool = channelPool;
            ChannelName = channelName;

            ReconnectionState = new ReconnectionTracker();

            // Ensuring settings are set
            Settings = settings ?? new Dictionary<string, object>();
            // Follow Json
            foreach (var s in new string[] { "durable", "exclusive", "autoDelete", "autoAck", "persistent" })
            {
                if (!Settings.ContainsKey(s))
                {
                    Settings[s] = false;
                }
            }
        }

        protected abstract IBasicProperties BuildChannelProperties(ChannelDecorator channelDecorator);

        protected abstract void EnsureExchangeDeclared(ChannelDecorator channelDecorator);

        public virtual bool PublishSafely(Action<ChannelDecorator> action)
        {
            var flag = false;

            if (!ReconnectionState.CanReconnect)
            {
                return flag;
            }

            var channelDecorator = ChannelPool.Get(ChannelName, ConnectionName);

            try
            {
                channelDecorator.PublishSafely(action);
                ReconnectionState.Reset();
                flag = true;
            }
            catch (UnexpectedConnectionDecoratorException)
            {
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
            }
            catch (UnexpectedChannelDecoratorException)
            {
                ChannelPool.Clear();

                ReconnectionState.BumpCounter();
            }
            catch (AlreadyClosedException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
            }
            catch (BrokerUnreachableException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
            }
            catch (ConnectFailureException)
            {
                ChannelPool.Clear();
                ConnectionPool.Clear();

                ReconnectionState.BumpCounter();
            }
            catch (Exception e)
            {
                ReconnectionState.BumpCounter();
            }

            return flag;
        }

        protected class ReconnectionTracker
        {
            public short ReconnectionCounter;
            public DateTime? LastFailureOn;

            public void Reset()
            {
                ReconnectionCounter = 0;
                LastFailureOn = null;
            }

            public void Fail()
            {
                LastFailureOn = DateTime.Now;
            }

            public void BumpCounter()
            {
                ReconnectionCounter++;
            }

            public bool CanReconnect
            {
                get
                {
                    if (ReconnectionCounter > 3)
                    {
                        return false;
                    }

                    // Within a short time, we have tried too many times.
                    if (LastFailureOn.HasValue)
                    {
                        // todo: some logic here
                    }

                    return true;
                }
            }
        }

    }
}
