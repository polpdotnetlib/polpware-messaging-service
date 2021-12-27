using System;

namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// Defines the extended interface for 
    /// replying to the incoming message, 
    /// compared to the subscription service.
    /// </summary>
    /// <typeparam name="TIn">Type of incoming message</typeparam>
    /// <typeparam name="TReply">Type of reply message</typeparam>
    public interface ISubscriptionWReplyService<TIn, TReply, TInter> : ISubscriptionService<TIn, TInter> 
        where TIn: class
        where TReply: class
        where TInter: class
    {
        /// <summary>
        /// Supplies the adatpor for translating the incoming message 
        /// into the outgoing one.
        /// </summary>
        /// <param name="func">Function to conduct the translation.</param>
        /// <returns>Service</returns>
        void SetReplyAdaptor(Func<TIn, TReply> func);
    }
}
