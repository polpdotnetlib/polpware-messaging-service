using System;

namespace Polpware.MessagingService.Protocol
{
    [Serializable]
    public class MessageContainer : IMessageContainer
    {
        public IMessageBody body { get; set; }
        public IMessageHead head { get; set; }

        public bool Accept(IMessageVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"head={head?.ToString()}\nbody={body?.ToString()};";
        }
    }
}
