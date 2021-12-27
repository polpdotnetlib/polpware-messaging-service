using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4DispatchingWReplyService<TIn, TReply, TInter> :
        Subscription4DispatchingService<TIn, TInter>, ISubscriptionWReplyService<TIn, TReply, TInter>
        where TIn : class 
        where TReply : class
        where TInter: class
    {
        protected Func<TIn, TReply> _replyAdaptor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionFactory">RabbitMQ-specific connection factory</param>
        /// <param name="exchange">Exchange name</param>
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues</param>
        /// <param name="queue">Specific queue, or leave it empty so that an anonymous, unique queue is generated</param>
        /// <param name="routingKey">The label used to characterize the group of a message to be sent out.</param>
        public Subscription4DispatchingWReplyService(ConnectionFactory connectionFactory, string exchange, IDictionary<string, object> settings, string queue, string routingKey)
            : base(connectionFactory, exchange, settings, queue, routingKey)
        { }


        /// <summary>
        /// Sets up the reply generator.
        /// It is required if a reply is wanted.
        /// Either this is invoked after the service is instantiated, 
        /// or it is preset in the constructor of the service.
        /// </summary>
        /// <param name="func">Function for generating the reply message</param>
        public void SetReplyAdaptor(Func<TIn, TReply> func)
        {
            _replyAdaptor = func;
        }

        protected void SendReply(TIn data, BasicDeliverEventArgs evt)
        {
            if (_replyAdaptor != null)
            {
                var replyProps = existingConnection.Channel.CreateBasicProperties();
                replyProps.CorrelationId = evt.BasicProperties.CorrelationId;

                var replyMessage = _replyAdaptor(data);

                var bytes = Polpware.Runtime.Serialization.ByteConvertor.ObjectToByteArray(replyMessage);

                existingConnection.Channel.BasicPublish(exchange: "",
                    routingKey: evt.BasicProperties.ReplyTo,
                                     basicProperties: replyProps,
                                     body: bytes);
            }
        }
    }
}
