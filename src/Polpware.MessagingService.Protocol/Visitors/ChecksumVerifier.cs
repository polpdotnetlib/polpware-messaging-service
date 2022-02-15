namespace Polpware.MessagingService.Protocol.Visitors
{
    public class ChecksumVerifier : IMessageSectionVisitor
    {
        public bool Visit(IMessageSection section)
        {
            if (section is IMessageContainer)
            {
                var container = section as IMessageContainer;
                return container.ReadBody().Md5() == container.ReadHead().md5;
            }
            return true;
        }
    }
}
