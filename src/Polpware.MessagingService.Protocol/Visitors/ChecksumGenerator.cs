namespace Polpware.MessagingService.Protocol.Visitors
{
    public class ChecksumGenerator : AbstractMessageVisitor
    {
        public override bool Visit(IMessageContainer container)
        {
            container.head.md5 = container.body.Md5();
            return true;
        }
    }
}
