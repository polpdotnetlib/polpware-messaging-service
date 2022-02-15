using Polpware.Configuration.Binder;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Polpware.MessagingService.Protocol
{
    public static class MessageConvertor
    {
        public static Dictionary<string, string> Object2Dict<U>(this U someObject) 
                  where U : class, new()
        {
            var input = new Dictionary<string, string>();
            var someObjectType = someObject.GetType();

            foreach (PropertyInfo propertyInfo in someObjectType.GetProperties())
            {
                var value = propertyInfo.GetValue(someObject);
                if (value != null)
                {
                    input[propertyInfo.Name] = value.ToString();
                }
            }

            return input;
        }

        /// <summary>
        /// Constructs a free message container for the given source.
        /// </summary>
        /// <typeparam name="T">Type of payload message</typeparam>
        /// <param name="source">Payload</param>
        /// <returns>Message container</returns>
        public static IMessageContainer BuildMessageFrom<T>(this T source)
            where T : class, new()
        {
            var dict = source.Object2Dict();
            var body = new MessageBody(dict);
            var head = new MessageHead();

            var container = new MessageContainer()
            {
                head = head,
                body = body
            };

            return container;
        }

        /// <summary>
        /// Constructs a message from the given head and body.
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="body">Body</param>
        /// <returns>Message container</returns>
        public static IMessageContainer BuildMessageFrom(IDictionary<string, string> head, IDictionary<string, string> body)
        {
            var dict1 = head.ToTypeSafeObject<MessageHead, string>();
            var dict2 = new MessageBody(body);
            var container = new MessageContainer()
            {
                head = dict1,
                body = dict2
            };

            return container;
        }

        /// <summary>
        /// Gets the message body from a dictionary.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="dict">Dictionary</param>
        /// <returns>Target object or null</returns>
        public static T GetMessageBody<T>(this IDictionary<string, string> dict)
            where T: class, new()
        {
            return dict.ToTypeSafeObject<T, string>();
        }

        /// <summary>
        /// Get the message body from a message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="func"></param>
        /// <returns>Target object or null</returns>
        public static T GetMessageBody<T>(this IMessageContainer container, Func<IMessageBody, T> func=null)
            where T: class, new()
        {
            if (func != null)
            {
                return func(container.ReadBody());
            }

            if (container.ReadBody() is MessageBody)
            {
                var s = container.ReadBody() as MessageBody;
                var t = s.ToTypeSafeObject<T, string>();

                return t;
            }

            return null;
        }


    }
}
