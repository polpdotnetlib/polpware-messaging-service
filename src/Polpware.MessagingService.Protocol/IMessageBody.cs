namespace Polpware.MessagingService.Protocol
{
    public interface IMessageBody : IMessageSection
    {
        string Md5();
    }
}
