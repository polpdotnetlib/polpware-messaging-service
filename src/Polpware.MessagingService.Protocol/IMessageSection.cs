namespace Polpware.MessagingService.Protocol
{
    public interface IMessageSection
    {
        bool Accept(IMessageSectionVisitor visitor);
    }
}
