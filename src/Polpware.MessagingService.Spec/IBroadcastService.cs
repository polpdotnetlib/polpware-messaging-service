namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A broadcast service
    /// </summary>
    /// <typeparam name="TOut">Type of sent data</typeparam>
    public interface IBroadcastService<TOut> where TOut: class
    {
        /// <summary>
        /// Broadcasts a message
        /// </summary>
        /// <param name="data">Payload</param>
        /// <returns>Success or failure</returns>
        bool BroadcastMessage(TOut data);
    }
}
