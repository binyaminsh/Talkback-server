using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TalkBack.Common;
using TalkBack.Users.Service.Dtos;
using TalkBack.Users.Service.Entities;

namespace TalkBack.Users.Service.SignalR.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserHub : Hub
    {
        private readonly IRepository<User> userRepository;

        // List client recieves
        private static List<UserModel> users = new();

        // Dictionary to manage connected users
        private static readonly Dictionary<string, string> onlineUsers = new();

        public UserHub(IRepository<User> userRepository)
        {
            this.userRepository = userRepository;

            // IN CASE APP IS STARTED WHILE THERE ARE USERS IN THE DB ALREADY
            // bug - the "users" list will only contain the online users and won't show the rest of the users as "offline"

            // UPDATE - fixed for now, but there should be a better way
        }

        private async Task GetUsers()
        {
            var allUsers = await userRepository.GetAllAsync();
            foreach (var user in allUsers)
            {
                if (!onlineUsers.ContainsValue(user.Id.ToString()) && users.FirstOrDefault(u => u.Id == user.Id) == null)
                {
                    users.Add(new UserModel
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        ChessRating = user.ChessRating,
                        IsOnline = false,
                    });
                }
            }
        }

        public async Task SendUsers()
        {
            await Clients.All.SendAsync("receiveUsers", users);
        }

        public override async Task OnConnectedAsync()
        {
            await GetUsers();

            var userConnectetionId = Context.ConnectionId;
            var userId = Context.GetUserIdentifier();

            // Gets the connected user details from the database
            var userDto = (await userRepository.GetAsync(userId)).AsDto();

            lock (onlineUsers)
            {
                onlineUsers.Add(userConnectetionId, userId.ToString());

                // Check if the single user already exists
                var existingUser = users.SingleOrDefault(user => user.Id == userId);
                if (existingUser != null)
                {
                    users[users.IndexOf(existingUser)].IsOnline = true;
                }
                else
                {
                    // TODO: Switch to automapper
                    users.Add(new UserModel
                    {
                        Id = userDto.Id,
                        DisplayName = userDto.DisplayName,
                        ChessRating = userDto.ChessRating,
                        IsOnline = true
                    });
                }
            }

            await SendUsers();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userConnectionId = Context.ConnectionId;
            var userId = Context.GetUserIdentifier();

            var existingUserIndex = users.FindIndex(user => user.Id == userId);

            lock (onlineUsers)
            {
                // Remove current user's connection from the Dictionary
                onlineUsers.Remove(userConnectionId);

                // Checking if user is disconnected from all devices by checking if his ID still appears in the Dictionary
                if (!onlineUsers.ContainsValue(userId.ToString()) && existingUserIndex != -1)
                    users[existingUserIndex].IsOnline = false;
            }

            await SendUsers();

            await base.OnDisconnectedAsync(exception);
        }
    }
}