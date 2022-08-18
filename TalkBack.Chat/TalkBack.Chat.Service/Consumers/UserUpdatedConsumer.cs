using MassTransit;
using TalkBack.Chat.Service.Entities;
using TalkBack.Common;
using TalkBack.Users.Contracts;

namespace TalkBack.Chat.Service.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdated>
    {
        private readonly IRepository<User> repository;

        public UserUpdatedConsumer(IRepository<User> repository)
        {
            this.repository = repository;
        }


        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;

            var user = await repository.GetAsync(message.Id);
            if (user == null)
            {
                user = new User
                {
                    Id = message.Id,
                    DisplayName = message.DisplayName,
                };

                await repository.CreateAsync(user);
            }
            else
            {
                user.DisplayName = message.DisplayName;

                await repository.UpdateAsync(user);
            }
        }
    }
}
