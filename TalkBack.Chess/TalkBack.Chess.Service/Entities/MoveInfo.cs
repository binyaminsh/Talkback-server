namespace TalkBack.Chess.Service.Entities
{
    public class MoveInfo
    {
        public string Move { get; set; } = null!;
        public string Piece { get; set; } = null!;
        public string Colour { get; set; } = null!;
        public bool X { get; set; }
        public bool Check { get; set; }
        public bool Stalemate { get; set; }
        public bool Checkmate { get; set; }
        public string Fen { get; set; } = null!;
        public string Pgn { get; set; } = null!;

        //public class PGN
        //{
        //    public string Pgn { get; set; } = null!;
        //}
    }
}
