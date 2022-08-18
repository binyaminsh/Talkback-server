using Microsoft.AspNetCore.SignalR;
using TalkBack.Users.Service.Dtos;
using TalkBack.Users.Service.Entities;

namespace TalkBack.Users.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this User user) => new(user.Id, user.DisplayName, user.ChessRating);

        public static Guid GetUserIdentifier(this HubCallerContext context)
        {
            var userIdentifier = context.UserIdentifier;
            if (userIdentifier == null)
                throw new HubException("ID is null");

            return Guid.Parse(userIdentifier);
        }
    }
}
