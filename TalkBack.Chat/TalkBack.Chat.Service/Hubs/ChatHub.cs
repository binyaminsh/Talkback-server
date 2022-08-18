using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TalkBack.Chat.Service.Dtos;
using TalkBack.Chat.Service.Entities;
using TalkBack.Common;

namespace TalkBack.Chat.Service.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly IRepository<User> userRepository;
        private readonly IRepository<Conversation> conversationRepository;

        public ChatHub(IRepository<User> userRepository, IRepository<Conversation> conversationRepository)
        {
            this.userRepository = userRepository;
            this.conversationRepository = conversationRepository;
        }

        // TODO: - cache message array per user instead of calling - angular
        //       - Display names can change, group names shouldn't be depended on them
        //       - Add indexing to messages in mongo for faster results

        public async Task JoinConversation(string recipientName, string recipientId)
        {
            var groupName = GetGroupName(Context.User.GetUsername(), recipientName);

            // Gets conversation from db
            Conversation conversation = await conversationRepository.GetAsync(conversation => conversation.GroupName == groupName);
            if (conversation != null)
            {
                // Notifies that the recipient is added to the conversation
                await Clients.User(recipientId).SendAsync("NotifyConversationCreated", conversation.Id);
                // Returns the conversation ID to the caller
                await Clients.Caller.SendAsync("GetConversationId", conversation.Id);
                return;
            }

            var user = await GetUserAsync();

            // In case a conversation doesn't exist
            conversation = new Conversation
            {
                Id = new Guid(),
                GroupName = groupName,
                Members = new List<User> { user }
            };

            // Saves the conversation in the db
            await conversationRepository.CreateAsync(conversation);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Notifies that the recipient is added to the conversation
            await Clients.User(recipientId).SendAsync("NotifyConversationCreated", conversation.Id);
            // Returns the conversation ID to the caller
            await Clients.Caller.SendAsync("GetConversationId", conversation.Id);
        }

        public async Task AddRecipientToConversation(string conversationId)
        {
            var conversation = await GetConversationAsync(conversationId);
            var user = await GetUserAsync();

            // checks if Members contain the user
            var existingUser = conversation.Members.SingleOrDefault(member => member.Id == user.Id);
            if (conversation != null && existingUser == null)
            {
                // in case the user is not found
                await Groups.AddToGroupAsync(Context.ConnectionId, conversation.GroupName);

                // Updates the List of members with the new member
                conversation.Members.Add(user);

                // Updates the Database
                await conversationRepository.UpdateAsync(conversation);
            }
        }

        private async Task AddUserToGroups()
        {
            var user = await GetUserAsync();

            // Gets all the conversations the user appears in
            var conversations = await conversationRepository.GetAllAsync(conversation => conversation.Members.Contains(user));

            // Checks if user exists in any conversations
            if (conversations.Count < 1)
                return;

            // Adds the user to the groups of all conversations he's a member of
            foreach (var group in conversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group.GroupName);
            }
        }

        public override async Task OnConnectedAsync()
        {
            // Add the user to groups he's a member of when connecting
            await AddUserToGroups();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task GetUserConversations()
        {
            var user = await GetUserAsync();
            var conversations = (await conversationRepository.GetAllAsync(conversation => conversation.Members.Contains(user)));

            if (conversations.Count < 1)
                return;

            await Clients.Caller.SendAsync("UserConversations", conversations.Select(conversation => conversation.AsDto()));
        }

        public async Task GetConversationHistory(string conversationId)
        {
            var conversation = await GetConversationAsync(conversationId);

            // Get all messages from a specific conversation
            var messages = conversation.Messages.Select(message => message.AsDto());
            await Clients.Caller.SendAsync("GetConversationMessages", messages);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var conversation = await GetConversationAsync(createMessageDto.ConversationId);

            var currentUserId = Context.GetUserId();

            var sender = conversation.Members.FirstOrDefault(user => user.Id == currentUserId);
            if (sender == null)
                throw new HubException("user not found in conversation");

            Message message = new()
            {
                Content = createMessageDto.Content,
                Sender = sender.DisplayName,
                TimeStamp = DateTimeOffset.UtcNow,
            };

            await Clients.Group(conversation.GroupName).SendAsync("NewMessage", message);
            await Clients.OthersInGroup(conversation.GroupName).SendAsync("NewMessageNotification", message.Sender);

            conversation.Messages.Add(message);
            await conversationRepository.UpdateAsync(conversation);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private async Task<User> GetUserAsync()
        {
            var user = await userRepository.GetAsync(Context.GetUserId());
            if (user == null)
                throw new HubException("User not found");

            return user;
        }
        private async Task<Conversation> GetConversationAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
                throw new HubException("conversation ID is empty");

            var conversation = await conversationRepository.GetAsync(Guid.Parse(conversationId));
            if (conversation == null)
                throw new HubException("Conversation not found");

            return conversation;
        }
    }
}