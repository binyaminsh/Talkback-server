using MassTransit;
using TalkBack.Chat.Service.Entities;
using TalkBack.Common;
using TalkBack.Users.Contracts;

namespace TalkBack.Chat.Service.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IRepository<User> repository;

        public UserCreatedConsumer(IRepository<User> repository)
        {
            this.repository = repository;
        }


        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var message = context.Message;

            // Make sure it haven't consumed the message already,
            // if for some reason the publisher publishes the same message twice
            var user = await repository.GetAsync(message.Id);
            if (user != null)
                return; // nothing else to do

            user = new User
            {
                Id = message.Id,
                DisplayName = message.DisplayName,
            };

            await repository.CreateAsync(user);
        }
    }
}
