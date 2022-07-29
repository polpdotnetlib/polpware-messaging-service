using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class RPCChannelFeature<TReturn>: IChannelCallbackFeature where TReturn : class
    {
        public string CallbackQueueName { get; private set; }
        public EventingBasicConsumer CallbackConsumer { get; private set; }

        public Func<string, TReturn> ReturnAdaptor { get; set; }
        public Action<TReturn> ReturnHandler { get; set; }
        public string CorrelationId { get; }

        private EventHandler<BasicDeliverEventArgs> _callbackDelegate;
        private bool _hooked;

        // Typically, leave it as empty or null. So the system will generate a unique ...
        public RPCChannelFeature(string replyQueue)
        {
            // Normlize
            CallbackQueueName = string.IsNullOrEmpty(replyQueue) ? Guid.NewGuid().ToString().ToUpper() : replyQueue.ToUpper();

            CorrelationId = Guid.NewGuid().ToString().ToUpper();

            ReturnAdaptor = x => x as TReturn;

            _callbackDelegate = (model, ea) =>
            {
                // todo: Deserialize into an object
                var body = ea.Body;
                // filter messages
                if (ea.BasicProperties.CorrelationId == CorrelationId)
                {
                    // todo: 
                    var payload = System.Text.Encoding.UTF8.GetString(body.ToArray());

                    var data = ReturnAdaptor(payload);
                    ReturnHandler?.Invoke(data);
                }

                // Auto Ack anyway
                // existingConnection.Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };
        }

        public void SetupCallback(ChannelDecorator channelDecorator)
        {
            if (!_hooked)
            {
                // Create our consumer
                CallbackConsumer = new EventingBasicConsumer(channelDecorator.Channel);

                CallbackConsumer.Received += _callbackDelegate;
                channelDecorator.RpcChannelFeature = this;
                _hooked = true;
            }
        }

        public void TearOffCallback(ChannelDecorator channelDecorator)
        {
            if (_hooked)
            {
                CallbackConsumer.Received -= _callbackDelegate;
                channelDecorator.RpcChannelFeature = null;
                _hooked = false;
            }
        }
    }
}
