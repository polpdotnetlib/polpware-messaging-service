namespace Polpware.MessagingService.Protocol
{
    public abstract class AbstractMessageVisitor : IMessageVisitor
    {
        public virtual bool Visit(IMessageHead head)
        {
            return true;
        }
        public virtual bool Visit(IMessageBody body)
        {
            return true;
        }
        public virtual bool Visit(IMessageContainer container)
        {
            if (Visit(container.head))
            {
                return Visit(container.body);
            }
            return false;
        }
    }
}
