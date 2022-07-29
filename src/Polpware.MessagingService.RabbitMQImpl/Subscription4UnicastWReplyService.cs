using Polpware.MessagingService.Spec;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public abstract class Subscription4UnicastWReplyService<TIn, TInter> : 
        Subscription4UnicastService<TIn, TInter>, ISubscriptionWReplyService<TIn, TInter>
        where TIn: class
        where TInter: class
    {
        protected Func<TIn, string> ReplyAdaptor;

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
        public void SetReplyAdaptor(Func<TIn, string> func)
        {
            ReplyAdaptor = func;
        }

        protected void SendReply(ChannelDecorator channelDecorator, TIn data, BasicDeliverEventArgs evt)
        {
            if (ReplyAdaptor != null)
            {
                var replyProps = channelDecorator.Channel.CreateBasicProperties();
                replyProps.CorrelationId = evt.BasicProperties.CorrelationId;

                var replyMessage = ReplyAdaptor(data);

                var bytes = System.Text.Encoding.UTF8.GetBytes(replyMessage);

                // todo: ????
                channelDecorator.Channel.BasicPublish(exchange: ExchangeName,
                    routingKey: evt.BasicProperties.ReplyTo,
                                     basicProperties: replyProps,
                                     body: bytes);
            }
        }

    }
}
