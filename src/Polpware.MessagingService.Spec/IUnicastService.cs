namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A unicast service.
    /// </summary>
    /// <typeparam name="TOut">Type of sent data</typeparam>
    public interface IUnicastService<TOut> where TOut : class
    {
        /// <summary>
        /// Sending out a message
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Success or failure</returns>
        bool SendMessage(TOut data);
    }
}
