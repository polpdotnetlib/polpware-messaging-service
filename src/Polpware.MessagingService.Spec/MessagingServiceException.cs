using System;

namespace Polpware.MessagingService.Spec
{
    /// <summary>
    /// A specific exception for messaging service.
    /// </summary>
    public class MessagingServiceException : Exception
    {
        public int Code { get; private set; }
        public MessagingServiceException(int code, string msg) : base(msg) {
            Code = code;
        }
        public MessagingServiceException(Exception inner, int code, string msg) : base(msg, inner)
        {
            Code = code;
        }
    }
}
