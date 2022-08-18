using TalkBack.Common;

namespace TalkBack.Login.Service.Entities
{
    public class LoginUser : IEntity
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;


    }
}
