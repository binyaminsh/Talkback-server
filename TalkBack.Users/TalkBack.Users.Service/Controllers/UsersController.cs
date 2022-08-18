using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalkBack.Common;
using TalkBack.Users.Contracts;
using TalkBack.Users.Service.Dtos;
using TalkBack.Users.Service.Entities;

namespace TalkBack.Users.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<User> usersRepository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IMapper mapper;

        public UsersController(IRepository<User> usersRepository, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            this.usersRepository = usersRepository;
            this.publishEndpoint = publishEndpoint;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAsync()
        {
            // Map each user to userDto
            var users = (await usersRepository.GetAllAsync())
                        .Select(user => user.AsDto());

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await usersRepository.GetAsync(id);

            if (user == null)
                return NotFound();

            return user.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> PostAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                Id = createUserDto.Id,
                DisplayName = createUserDto.DisplayName,
                ChessRating = 1200,
                CreatedDate = DateTimeOffset.UtcNow,
            };

            await usersRepository.CreateAsync(user);

            // publishes a message via mass transit to rabbitMQ to notify all listeners that a user was created
            await publishEndpoint.Publish(new UserCreated(user.Id, user.DisplayName, user.ChessRating));

            return CreatedAtAction(nameof(GetByIdAsync), new {id = user.Id}, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var existingUser = await usersRepository.GetAsync(id);

            if (existingUser == null)
                return NotFound();

            //existingUser.DisplayName = updateUserDto.DisplayName;
            // Maps without creating a new object
            existingUser = mapper.Map<UpdateUserDto, User>(updateUserDto, existingUser);

            await usersRepository.UpdateAsync(existingUser);

            // publishes a message via mass transit to rabbitMQ to notify all listeners that a user was updated
            await publishEndpoint.Publish(new UserUpdated(existingUser.Id, existingUser.DisplayName, existingUser.ChessRating));

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var user = await usersRepository.GetAsync(id);

            if (user == null)
                return NotFound();

            await usersRepository.RemoveAsync(id);
            
            // publishes a message via mass transit to rabbitMQ to notify all listeners that a user was deleted
            await publishEndpoint.Publish(new UserDeleted(id));

            // TODO: check what to return
            return NoContent();
        }
    }
}
