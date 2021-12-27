using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class AbstractConnection : IDisposable
    {
        protected ConnectionBuilder existingConnection { get; private set; }
        protected readonly ReconnectionTracker reconnectionTracker;

        protected readonly IDictionary<string, object> _settings;

        private readonly ConnectionFactory _connectionFactory;

        public AbstractConnection(ConnectionFactory connectionFactory, IDictionary<string, object> settings)
        {
            reconnectionTracker = new ReconnectionTracker();
            _connectionFactory = connectionFactory;

            // Ensuring settings are set
            _settings = settings ?? new Dictionary<string, object>();
            foreach (var s in new string[] { "durable", "exclusive", "autoDelete", "autoAck", "persistent" })
            {
                if (!_settings.ContainsKey(s))
                {
                    _settings[s] = false;
                }
            }

            existingConnection = new ConnectionBuilder(connectionFactory);
        }

        public void Dispose()
        {
            existingConnection?.Dispose();
        }

        public virtual bool ReBuildConnection()
        {
            // Dispose whatever we have before.
            try
            {
                this.Dispose();
            } catch (Exception)
            {

            }

            if (this.reconnectionTracker.CanReconnect)
            {
                return false;
            }

            this.reconnectionTracker.BumpCounter();

            this.existingConnection = new ConnectionBuilder(this._connectionFactory);

            return true;
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
                    if (this.ReconnectionCounter > 3)
                    {
                        return false;
                    }

                    // Within a short time, we have tried too many times.
                    if (this.LastFailureOn.HasValue)
                    {
                        // todo: some logic here
                    }

                    return true;
                }
            }
        }

    }
}
