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
        protected Func<TIn, TReply> ReplyAdaptor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionPool">Connection pool</param>
        /// <param name="channelPool">Channel pool</param>
        /// <param name="connectionName">Connection name</param>
        /// <param name="channelName">Channel name</param>
        /// <param name="exchange">Exchange name</param> 
        /// <param name="settings">A set of settings, such as  durable, persistent, exclusive, autoDelete, autoAck for queues</param>
        /// <param name="queue">Specific queue, or leave it empty so that an anonymous, unique queue is generated</param>
        /// <param name="routingKey">The label used to characterize the group of a message to be sent out.</param>
        public Subscription4DispatchingWReplyService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange, 
            IDictionary<string, object> settings, string queue, string routingKey)
            : base(connectionPool, channelPool, connectionName, channelName, exchange, settings, queue, routingKey)
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
            ReplyAdaptor = func;
        }

        protected void SendReply(ChannelDecorator channelDecorator, TIn data, BasicDeliverEventArgs evt)
        {
            // todo: Check the correctness
            if (ReplyAdaptor != null)
            {
                var replyProps = channelDecorator.Channel.CreateBasicProperties();
                replyProps.CorrelationId = evt.BasicProperties.CorrelationId;

                var replyMessage = ReplyAdaptor(data);

                var bytes = Polpware.Runtime.Serialization.ByteConvertor.ObjectToByteArray(replyMessage);

                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                    routingKey: evt.BasicProperties.ReplyTo,
                    basicProperties: replyProps,
                    body: bytes);
            }
        }
    }
}
