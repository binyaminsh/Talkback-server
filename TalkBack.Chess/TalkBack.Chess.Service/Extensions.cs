using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TalkBack.Chess.Service.Dtos;
using TalkBack.Chess.Service.Entities;

namespace TalkBack.Chess.Service
{
    public static class Extensions
    {
        public static GameDto AsDto(this Game game) => new(game.Id, game.WhitePlayer, game.BlackPlayer);

        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username = user?.Identity?.Name;
            if (username == null)
                throw new HubException("username is null");

            return username;
        }

        public static Guid GetUserId(this HubCallerContext context)
        {
            var userIdentifier = context.UserIdentifier;
            if (userIdentifier == null)
                throw new HubException("ID is null");

            return Guid.Parse(userIdentifier);
        }
    }
}
