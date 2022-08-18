using TalkBack.Common;

namespace TalkBack.Chess.Service.Entities
{
    public class Game : IEntity
    {
        public Guid Id { get; set; }

        public string WhitePlayer { get; set; } = null!;
        public string WhitePlayerId { get; set; } = null!;
        public string BlackPlayer { get; set; } = null!;
        public string BlackPlayerId { get; set; } = null!;
        public ICollection<MoveInfo> Moves { get; set; } = new List<MoveInfo>();
        public string Result { get; set; } = null!;

        // result 0 white won, 1 black
    }
}
