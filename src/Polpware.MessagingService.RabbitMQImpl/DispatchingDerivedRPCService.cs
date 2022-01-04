using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    /// <summary>
    /// A service for dispatching messages out, while expecting replies to those messages. 
    /// Thus, it resembles a RPC-like messaging pattern, underpinned by the dispatching service. 
    /// </summary>
    /// <typeparam name="TCall">Message to be sent out, while the replied message is an Object.</typeparam>
    public class DispatchingDerivedRPCService<TCall, TReturn> : DispatchingService<TCall>, IRPCLike<TCall, TReturn> 
        where TCall : class
        where TReturn: class
    {
        private RPCInitHelper<TReturn> _RPCInitHelper;
        private readonly string _replyQueue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionFactory">RabbitMQ-specific connection factory</param>
        /// <param name="exchange">Exchange name</param>
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues. 
        /// Amongest them, durable, persistent, exclusive, and autoDelete are used to define the properties of queue,
        /// while autoAck is to define the properties of the consumer (event handler).</param>
        /// <param name="replyQueue">Specific queue for replying messages. Most time leave it empty so that an anonymous queue will be computed internally.</param>
        public DispatchingDerivedRPCService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings, string replyQueue) : 
            base(connectionFactory, exchange, settings)
        {
            _replyQueue = replyQueue;
        }

        protected override void Init()
        {
            base.Init();

            _RPCInitHelper = new RPCInitHelper<TReturn>(_replyQueue);
            _RPCInitHelper.InitCallback(existingConnection);
        }

        protected override void PrepareProperties()
        {
            base.PrepareProperties();

            Props.CorrelationId = _RPCInitHelper.CorrelationId;
            Props.ReplyTo = _RPCInitHelper.CallbackQueueName;
        }

        public override bool ReBuildConnection()
        {
            var f = base.ReBuildConnection();

            _RPCInitHelper.InitCallback(existingConnection);
            return f;
        }

        public void SetReturnAdaptor(Func<object, TReturn> func)
        {
            _RPCInitHelper.ReturnAdaptor = func;
        }

        public void SetReturnHandler(Action<TReturn> action)
        {
            _RPCInitHelper.ReturnHandler = action;
        }

        public void Call(TCall data, params object[] options)
        {
            var routingKey = options.Length > 0 ? options[0].ToString() : "";
            DispatchMessage(data, routingKey);

            existingConnection.Channel.BasicConsume(_RPCInitHelper.CallbackConsumer,
                queue: _RPCInitHelper.CallbackQueueName,
                autoAck: true);
        }
    }
}
