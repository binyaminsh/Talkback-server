using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Text.Json;
using TalkBack.Chess.Service.Entities;
using TalkBack.Common;

namespace TalkBack.Chess.Service.SignalR.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChessHub : Hub
    {
        private readonly IRepository<Game> gameRepository;
        static readonly List<Game> currentGames = new();

        public ChessHub(IRepository<Game> gameRepository)
        {
            this.gameRepository = gameRepository;
        }


        public async Task InviteToChess(string recipientId)
        {
            var senderName = Context.User.GetUsername();

            await Clients.User(recipientId).SendAsync("chessinvitation", senderName);
            await Clients.Caller.SendAsync("waitingForInvitationResponseMessage");
        }

        public async Task CreateGame(string recipientName, string recipientId)
        {
            var senderName = Context.User.GetUsername();

            //var groupName = GetGroupName(Context.User?.Identity?.Name, recipientName);
            foreach (var currentGame in currentGames)
            {
                if (senderName == currentGame.WhitePlayer || senderName == currentGame.BlackPlayer
                    || recipientName == currentGame.WhitePlayer || recipientName == currentGame.BlackPlayer)
                {
                    await Clients.Caller.SendAsync("userAlreadyInAGame");
                    return;
                }
            }

            Game game = new()
            {
                Id = Guid.NewGuid(),
                WhitePlayer = Context.User.GetUsername(),
                WhitePlayerId = Context.GetUserId().ToString(),
                BlackPlayer = recipientName,
                BlackPlayerId = recipientId,
            };

            currentGames.Add(game);

            await Clients.Caller.SendAsync("RoomCreated", game.Id);
            await Clients.User(recipientId).SendAsync("RoomCreated", game.Id);

            await InviteToChess(recipientId);
        }

        public async Task OnInvitationAccept(string gameId)
        {
            var game = GetCurrentGame(gameId);

            // Get the id of the opponnent
            var opponnentId = Context.GetUserId().ToString() == game.WhitePlayerId ? game.BlackPlayerId : game.WhitePlayerId;

            //await Clients.User(opponnentId).SendAsync("InvitationAccepted");

            // picks colour in a שרירותי way
            // TODO: Randomize colours
            await Clients.User(game.WhitePlayerId).SendAsync("InvitationAccepted", game.AsDto(), "white");
            await Clients.User(game.BlackPlayerId).SendAsync("InvitationAccepted", game.AsDto(), "black");
        }

        public async Task OnInvitationDecline(string gameId)
        {
            var game = GetCurrentGame(gameId);

            var opponnentId = Context.GetUserId().ToString() == game.WhitePlayerId ? game.BlackPlayerId : game.WhitePlayerId;

            await Clients.User(opponnentId).SendAsync("InvitationDeclined");
            currentGames.Remove(game);
        }

        public async Task PieceMove(JsonDocument move, string opponentName, string gameId)
        {
            var game = GetCurrentGame(gameId);

            var opponnentId = Context.GetUserId().ToString() == game.WhitePlayerId ? game.BlackPlayerId : game.WhitePlayerId;
            var moveInfoObject = ConvertJSONMoveToMoveInfoObject(move);

            string a = JsonSerializer.Serialize(move);
            await Clients.User(opponnentId).SendAsync("move", a);


            game.Moves.Add(new MoveInfo
            {
                Move = moveInfoObject.Move,
                Piece = moveInfoObject.Piece,
                Check = moveInfoObject.Check,
                Checkmate = moveInfoObject.Checkmate,
                Colour = moveInfoObject.Color,
                Fen = moveInfoObject.Fen,
                Stalemate = moveInfoObject.Stalemate,
                X = moveInfoObject.X,
                Pgn = moveInfoObject.Pgn.Pgn,
            });

            if (moveInfoObject.Checkmate || moveInfoObject.Stalemate)
            {
                string reason = moveInfoObject.Checkmate ? $"{moveInfoObject.Color} won" : "stalemate";
                await GameOver(game, reason);
            }
        }

        public async Task GetGamePgnAndFen(string gameId)
        {
            var game = await GetGameAsync(gameId);

            var lastMove = game.Moves.LastOrDefault();
            if (lastMove == null)
                return;

            await Clients.Caller.SendAsync("getPgnAndFen", lastMove.Pgn, lastMove.Fen);
        }

        public async Task GameOver(Game game, string reason)
        {
            await Clients.Users(game.WhitePlayerId, game.BlackPlayerId).SendAsync("gameOver", reason);
            game.Result = reason;

            // TODO: Check double call?
            await gameRepository.CreateAsync(game);
            currentGames.Remove(game);
        }

        public async Task Resign(string gameId, string reason)
        {
            // TODO: Add "reason" logic here and not via angular
            var game = GetCurrentGame(gameId);
            await GameOver(game, reason);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var game in currentGames)
            {
                if (game.WhitePlayerId == Context.GetUserId().ToString())
                {
                    await GameOver(game, $"{Context.User.GetUsername()} left the match, {game.BlackPlayer} won");
                    return;
                }
                else if (game.BlackPlayerId == Context.GetUserId().ToString())
                {
                    await GameOver(game, $"{Context.User.GetUsername()} left the match, {game.WhitePlayer} won");
                    return;
                }
            }

            // TODO: check is it takin?
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageInChess(string content, string recipientName, string gameId)
        {
            var game = await GetGameAsync(gameId);

            var sender = Context.User!.GetUsername();
            var message = new ChessMessage
            {
                Sender = sender,
                Content = content
            };

            await Clients.Users(game.WhitePlayerId, game.BlackPlayerId).SendAsync("NewMessageInChessChat", message);
        }

        private Game GetCurrentGame(string gameId)
        {
            var game = currentGames.FirstOrDefault(game => game.Id == Guid.Parse(gameId));
            return game ?? throw new HubException("game is null");
        }

        private async Task<Game> GetGameAsync(string gameId)
        {
            var game = currentGames.FirstOrDefault(game => game.Id == Guid.Parse(gameId));
            game ??= await gameRepository.GetAsync(Guid.Parse(gameId));
            return game ?? throw new HubException("game is null");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }


        //private void RandomizeSides()
        //{
        //    Random rnd = new();

        //    var side = rnd.Next(0, 1);
        //    if (side == 0)
        //    {
        //        game.WhitePlayerId = Context.GetUserId().ToString();
        //        game.WhitePlayer = Context.User.GetUsername();

        //    }
        //}

        private MoveInfoObject ConvertJSONMoveToMoveInfoObject(JsonDocument move)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var moveInfo = move.RootElement.Deserialize<MoveInfoObject>(options);
            if (moveInfo == null)
                throw new HubException("Failed to deserialize move");

            return moveInfo;
        }
    }
}
