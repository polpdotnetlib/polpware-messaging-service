using System;

namespace Polpware.MessagingService.Protocol
{
    [Serializable]
    public class MessageHead : IMessageHead
    {
        public string name { get; set; }
        public string address { get; set; }
        public string app_id { get; set; }
        public string md5 { get; set; }
        public string auth { get; set; }

        public bool Accept(IMessageSectionVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"name={name?.ToString()};address={address?.ToString()};app_id={app_id?.ToString()};md5={md5?.ToString()};auth={auth?.ToString()}";
        }
    }
}
