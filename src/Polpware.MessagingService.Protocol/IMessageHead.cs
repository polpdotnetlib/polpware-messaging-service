namespace Polpware.MessagingService.Protocol
{
    public interface IMessageHead : IMessageSection
    {
        string name { get; }
        string address { get; }
        string app_id { get; }
        string md5 { get; set; }
        string auth { get; }
    }
}
