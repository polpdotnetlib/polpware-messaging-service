using System;

namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A RPC-like interface
    /// </summary>
    public interface IRPCLike<TCall, IReturn> 
        where TCall : class
        where IReturn : class
    {
        /// <summary>
        /// Set the data adaptor for translating 
        /// the incoming message into an 
        /// object of the incoming data type.
        /// </summary>
        /// <param name="func">Function</param>
        void SetReturnAdaptor(Func<object, IReturn> func);

        /// <summary>
        /// Sets the data handler for the incoming message.
        /// </summary>
        /// <param name="action">Callback</param>
        void SetReturnHandler(Action<IReturn> action);

        /// <summary>
        /// Invokes a remote call.
        /// The callback needs to be set beforehand.
        /// </summary>
        /// <param name="data">Data to be sent</param>
        /// <param name="options">Options</param>
        void Call(TCall data, params object[] options);
    }
}
