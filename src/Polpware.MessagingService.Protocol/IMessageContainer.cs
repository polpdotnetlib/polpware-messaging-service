namespace Polpware.MessagingService.Protocol
{
    public interface IMessageContainer : IMessageSection
    {
        IMessageBody ReadBody();
        IMessageHead ReadHead();
    }
}
