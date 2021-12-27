using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class UnicastDerivedRPCService<TCall, TReturn> : UnicastService<TCall>, IRPCLike<TCall, TReturn> 
        where TCall : class
        where TReturn: class
    {
        private RPCInitHelper<TReturn> _RPCInitHelper;
        private readonly string _replyQueue;

        public UnicastDerivedRPCService(ConnectionFactory connectionFactory, string queue, IDictionary<string, object> settings, string replyQueue) 
            : base(connectionFactory, queue, settings)
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

            _props.CorrelationId = _RPCInitHelper.CorrelationId;
            _props.ReplyTo = _RPCInitHelper.CallbackQueueName;
        }

        public override bool ReBuildConnection()
        {
            var f = base.ReBuildConnection();

            _RPCInitHelper.InitCallback(existingConnection);
            return f;
        }

        public void Call(TCall data, params object[] options)
        {
            SendMessage(data);

            // auto ack
            existingConnection.Channel.BasicConsume(_RPCInitHelper.CallbackConsumer, 
                queue: _RPCInitHelper.CallbackQueueName,
                autoAck: true);
        }

        public void SetReturnAdaptor(Func<object, TReturn> func)
        {
            _RPCInitHelper.ReturnAdaptor = func;
        }

        public void SetReturnHandler(Action<TReturn> action)
        {
            _RPCInitHelper.ReturnHandler = action;
        }
    }
}
