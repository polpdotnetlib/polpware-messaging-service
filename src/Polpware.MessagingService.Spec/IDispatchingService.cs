namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A dispatching service. 
    /// Contrast to broadcast, dispatching allow for fine grained 
    /// control on subscribing for a subset of messages sent out 
    /// by a producer.
    /// </summary>
    /// <typeparam name="TOut">Type of sent data</typeparam>
    public interface IDispatchingService<TOut> where TOut : class
    {
        /// <summary>
        /// Dispatch a message. Though some specific implementation does not exploit 
        /// the type of the given data, some other impldmentions may use this information. 
        /// Thus, we pass the exact type information.
        /// </summary>
        /// <param name="data">Message</param>
        /// <param name="routingKey">A label used to identify the kind of messages.</param>
        /// <returns>Success or failure</returns>
        bool DispatchMessage(TOut data, string routingKey);
    }
}
