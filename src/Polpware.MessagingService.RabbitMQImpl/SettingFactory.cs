using System.Collections.Generic;

namespace Polpware.MessagingService.RabbitMQImpl
{
    public class SettingFactory
    {
        /// <summary>
        /// Creates a new set of settings for broadcast service.
        /// Default value: 
        /// Durable: true
        /// persistent: true
        /// exclusive: false
        /// autoDelete: false
        /// autoAck: true
        /// </summary>
        /// <returns>Settings</returns>
        public static Dictionary<string, object> NewDefaultBroadcastSettings(bool durable = true,
            bool persistence = true,
            bool exclusive = false,
            bool autoDelete = false,
            bool autoAck = true)
        {
            return new Dictionary<string, object>(){
                { "durable", durable },
                { "persistent", persistence },
                { "exclusive", exclusive },
                { "autoDelete", autoDelete},
                { "autoAck", autoAck }
            };
        }

        /// <summary>
        /// Creates a new set of settings for dispatching service.
        /// Default value: 
        /// Durable: true
        /// persistent: true
        /// exclusive: false
        /// autoDelete: false
        /// autoAck: false
        /// </summary>
        /// <returns>Settings</returns>
        public static Dictionary<string, object> NewDefaultDispatchingSettings(bool durable = true,
            bool persistence = true,
            bool exclusive = false,
            bool autoDelete = false,
            bool autoAck = false)
        {
            return new Dictionary<string, object>(){
                { "durable", durable },
                { "persistent", persistence },
                { "exclusive", exclusive },
                { "autoDelete", autoDelete},
                { "autoAck", autoAck }
            };
        }

        /// <summary>
        /// Creates a new set of settings for unicast service.
        /// Default value: 
        /// Durable: true
        /// persistent: true
        /// exclusive: false
        /// autoDelete: false
        /// autoAck: false
        /// </summary>
        /// <returns>Settings</returns>
        public static Dictionary<string, object> NewDefaultUnicastSettings(bool durable = true,
            bool persistence = true,
            bool exclusive = false,
            bool autoDelete = false,
            bool autoAck = false)
        {
            return new Dictionary<string, object>(){
                { "durable", durable },
                { "persistent", persistence },
                { "exclusive", exclusive },
                { "autoDelete", autoDelete},
                { "autoAck", autoAck }
            };
        }
    }
}
