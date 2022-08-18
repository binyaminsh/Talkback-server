namespace TalkBack.Users.Service.SignalR
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = null!;

        public int ChessRating { get; set; } 

        public bool IsOnline { get; set; } 
    }
}