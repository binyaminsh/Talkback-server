using TalkBack.Common;

namespace TalkBack.Chat.Service.Entities
{
    public class Conversation : IEntity
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = null!;
        public List<User> Members { get; set; } = null!;
        public List<Message> Messages { get; set; } = new(); 
    }
}
