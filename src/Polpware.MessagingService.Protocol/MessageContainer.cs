using System;

namespace Polpware.MessagingService.Protocol
{
    [Serializable]
    public class MessageContainer : IMessageContainer
    {
        public MessageBody body { get; set; }
        public MessageHead head { get; set; }


        public bool Accept(IMessageSectionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public IMessageBody ReadBody()
        {
            return body;
        }

        public IMessageHead ReadHead()
        {
            return head;
        }

        public override string ToString()
        {
            return $"head={head?.ToString()}\nbody={body?.ToString()};";
        }
    }
}
