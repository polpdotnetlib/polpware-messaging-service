namespace Polpware.MessagingService.Protocol
{
    public interface IMessageVisitor
    {
        bool Visit(IMessageHead head);
        bool Visit(IMessageBody body);
        bool Visit(IMessageContainer container);
    }
}
