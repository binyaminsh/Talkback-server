using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TalkBack.Chat.Service.Dtos;
using TalkBack.Chat.Service.Entities;

namespace TalkBack.Chat.Service
{
    public static class Extensions
    {
        public static MessageDto AsDto(this Message message) => new(/*message.Id,*/ message.Sender, /*message.Recievers,*/ message.Content, message.TimeStamp);
        public static ConversationDto AsDto(this Conversation conversation) => new(conversation.Id, conversation.GroupName);
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
