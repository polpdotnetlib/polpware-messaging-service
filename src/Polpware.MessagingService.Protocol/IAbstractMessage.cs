namespace Polpware.MessagingService.Protocol
{
    public interface IAbstractMessage
    {
        bool Accept(IMessageVisitor visitor);
    }
}
