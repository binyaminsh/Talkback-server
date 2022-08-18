using MassTransit;
using TalkBack.Chat.Service.Entities;
using TalkBack.Common;
using TalkBack.Users.Contracts;

namespace TalkBack.Chat.Service.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeleted>
    {
        private readonly IRepository<User> repository;

        public UserDeletedConsumer(IRepository<User> repository)
        {
            this.repository = repository;
        }


        public async Task Consume(ConsumeContext<UserDeleted> context)
        {
            var message = context.Message;

            var user = await repository.GetAsync(message.Id);
            if (user == null)
                return;
            
            await repository.RemoveAsync(user.Id);
        }
    }
}
