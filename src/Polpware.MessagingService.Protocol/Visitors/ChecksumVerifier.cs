namespace Polpware.MessagingService.Protocol.Visitors
{
    public class ChecksumVerifier : AbstractMessageVisitor
    {
        public override bool Visit(IMessageContainer container)
        {
            return container.body.Md5() == container.head.md5;
        }
    }
}
