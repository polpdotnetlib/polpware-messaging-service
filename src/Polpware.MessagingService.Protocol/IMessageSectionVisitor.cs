namespace Polpware.MessagingService.Protocol
{
    public interface IMessageSectionVisitor
    {
        bool Visit(IMessageSection section);
    }
}
