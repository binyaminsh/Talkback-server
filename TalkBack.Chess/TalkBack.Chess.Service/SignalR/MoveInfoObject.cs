namespace TalkBack.Chess.Service.SignalR
{

    public class MoveInfoObject
    {
        public string Move { get; set; } = null!;
        public string Piece { get; set; } = null!;
        public string Color { get; set; } = null!;
        public bool X { get; set; }
        public bool Check { get; set; }
        public bool Stalemate { get; set; }
        public bool Mate { get; set; }
        public bool Checkmate { get; set; }
        public string Fen { get; set; } = null!;
        public PGN Pgn { get; set; } = null!;
        public bool FreeMode { get; set; }
    }

    public class PGN
    {
        public string Pgn { get; set; } = null!;
    }

}
