namespace Polpware.MessagingService.Protocol
{
    public interface IMessageBody : IAbstractMessage
    {
        string Md5();
    }
}
