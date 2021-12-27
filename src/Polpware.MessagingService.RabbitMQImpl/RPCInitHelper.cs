using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class RPCInitHelper<TReturn> where TReturn : class
    {
        public readonly bool _isAnonymousReplyQueue;
        public string CallbackQueueName;
        public EventingBasicConsumer CallbackConsumer;

        public Func<object, TReturn> ReturnAdaptor;
        public Action<TReturn> ReturnHandler;
        public readonly string CorrelationId;

        public RPCInitHelper(string replyQueue)
        {
            CallbackQueueName = replyQueue;
            _isAnonymousReplyQueue = string.IsNullOrEmpty(replyQueue);
            CorrelationId = Guid.NewGuid().ToString();

            ReturnAdaptor = x => x as TReturn;
        }

        public void InitCallback(ConnectionBuilder existingConnection)
        {

            if (_isAnonymousReplyQueue)
            {
                // Create one
                CallbackQueueName = existingConnection.Channel.QueueDeclare().QueueName;
            }

            // Create our consumer
            CallbackConsumer = new EventingBasicConsumer(existingConnection.Channel);

            CallbackConsumer.Received += (model, ea) =>
            {
                // todo: Deserialize into an object
                var body = ea.Body;
                // filter messages
                if (ea.BasicProperties.CorrelationId == CorrelationId)
                {
                    // todo: 
                    var payload = Runtime.Serialization.ByteConvertor.ByteArrayToObject(body);

                    var data = ReturnAdaptor(payload);
                    ReturnHandler?.Invoke(data);
                }

                // Auto Ack anyway
                // existingConnection.Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };
        }
    }
}
