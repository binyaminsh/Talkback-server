using TalkBack.Common;

namespace TalkBack.Users.Service.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public int ChessRating { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
