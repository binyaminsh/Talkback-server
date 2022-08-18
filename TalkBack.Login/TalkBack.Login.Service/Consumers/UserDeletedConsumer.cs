using MassTransit;
using TalkBack.Common;
using TalkBack.Login.Service.Entities;
using TalkBack.Users.Contracts;

namespace TalkBack.Login.Service.Consumers
{
    public class UserDeletedConsumer : IConsumer<UserDeleted>
    {
        private readonly IRepository<LoginUser> repository;

        public UserDeletedConsumer(IRepository<LoginUser> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<UserDeleted> context)
        {
            var message = context.Message;

            var user = await repository.GetAsync(message.Id);
            if (user == null)
                return;

            await repository.RemoveAsync(message.Id);
        }
    }
}
