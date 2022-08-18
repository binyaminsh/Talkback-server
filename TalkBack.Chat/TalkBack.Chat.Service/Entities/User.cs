using TalkBack.Common;

namespace TalkBack.Chat.Service.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = null!;
    }
}
