namespace Polpware.MessagingService.Protocol.Visitors
{
    public class ChecksumGenerator : IMessageSectionVisitor
    {
        public bool Visit(IMessageSection section)
        {
            if (section is IMessageHead)
            {
                var container = section as MessageContainer;
                container.ReadHead().md5 = container.ReadBody().Md5();
            }
            return true;
        }
    }
}
