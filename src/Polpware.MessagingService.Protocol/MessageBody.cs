using System.Collections.Generic;

namespace Polpware.MessagingService.Protocol
{
    public class MessageBody : Dictionary<string, string>, IMessageBody
    {
        public MessageBody()
        {
        }

        public MessageBody(IDictionary<string, string> dict) : base(dict)
        {
        }

        public bool Accept(IMessageVisitor visitor)
        {
            visitor.Visit(this);
            return true;
        }

        public string Md5()
        {
            return "md5";
        }
    }
}
