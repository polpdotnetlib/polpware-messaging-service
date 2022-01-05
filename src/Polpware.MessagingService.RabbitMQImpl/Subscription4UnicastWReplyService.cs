using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4UnicastWReplyService<TIn, TReply, TInter> : 
        Subscription4UnicastService<TIn, TInter>, ISubscriptionWReplyService<TIn, TReply, TInter>
        where TIn: class
        where TReply: class
        where TInter: class
    {
        protected Func<TIn, TReply> ReplyAdaptor;

        public Subscription4UnicastWReplyService(IConnectionPool connectionPool,
            IChannelPool channelPool,
            string connectionName,
            string channelName,
            string exchange,
            string queue, 
            IDictionary<string, object> settings)
            :base(connectionPool, channelPool, connectionName, channelName, exchange, queue, settings)
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

        protected void SendReply(TIn data, BasicDeliverEventArgs evt)
        {
            if (ReplyAdaptor != null)
            {
                var replyProps = EffectiveChannelDecorator.Channel.CreateBasicProperties();
                replyProps.CorrelationId = evt.BasicProperties.CorrelationId;

                var replyMessage = ReplyAdaptor(data);

                var bytes = Polpware.Runtime.Serialization.ByteConvertor.ObjectToByteArray(replyMessage);

                // todo: ????
                EffectiveChannelDecorator.Channel.BasicPublish(exchange: "",
                    routingKey: evt.BasicProperties.ReplyTo,
                                     basicProperties: replyProps,
                                     body: bytes);
            }
        }

    }
}
