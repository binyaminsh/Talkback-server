using TalkBack.Common;

namespace TalkBack.Chat.Service.Entities
{
    public class Message    /* : IEntity*/
    {
        //public Guid Id { get; set; }
        public string Sender { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTimeOffset TimeStamp { get; set; }
    }
}
