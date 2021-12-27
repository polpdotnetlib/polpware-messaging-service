namespace Polpware.MessagingService.Protocol
{
    public interface IMessageContainer : IAbstractMessage
    {
        IMessageBody body { get; }
        IMessageHead head { get; }
    }
}
