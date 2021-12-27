using System;

namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A general subscription service
    /// </summary>
    public interface ISubscriptionService<TIn, TInter> 
        where TIn : class
        where TInter : class
    {
        /// <summary>
        /// Subscribes with the given logic to be run on receiving a message.
        /// If no adaptor or callback is given, use the built-in one.
        /// </summary>
        /// <param name="adaptor">Adaptor</param>
        /// <param name="handler">Callback</param>
        void Subscribe(Func<object, Tuple<TIn, TInter>> adaptor = null, Func<TIn, TInter, int> handler = null);

        /// <summary>
        /// Sets the adatpor
        /// </summary>
        /// <param name="adaptor">Function to translate the incoming message into a type-safe object.</param>
        /// <returns>Service</returns>
        void SetDataAdaptor(Func<object, Tuple<TIn, TInter>> adaptor);

        /// <summary>
        /// Sets the callback
        /// </summary>
        /// <param name="callback">Function to utilize the translated incoming message</param>
        /// <returns>Service</returns>
        void SetDataHandler(Func<TIn, TInter, int> callback);

        /// <summary>
        /// Sets the handler for exception handler. 
        /// The return value is used to 
        /// </summary>
        /// <param name="handler"></param>
        void SetExceptionHandler(Func<Exception, Tuple<bool, bool>> handler);

        /// <summary>
        /// Checks if the subscription is running or not.
        /// </summary>
        /// <returns>True or false</returns>
        bool IsOperating { get; }
    }
}
