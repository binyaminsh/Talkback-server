using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TalkBack.Common;
using TalkBack.Login.Service.Clients;
using TalkBack.Login.Service.Dtos;
using TalkBack.Login.Service.Entities;
using TalkBack.Login.Service.Services.PasswordHash;
using TalkBack.Login.Service.Services.TokenGenerators;

namespace TalkBack.Login.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepository<LoginUser> loginRepository;
        private readonly IJwtTokenGenerator jwtTokenGenerator;
        private readonly IPasswordHasher passwordHasher;
        private readonly UserClient userClient;

        public AuthController(IRepository<LoginUser> loginRepository, IJwtTokenGenerator jwtTokenGenerator, UserClient userClient, IPasswordHasher passwordHasher)
        {
            this.loginRepository = loginRepository;
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.userClient = userClient;
            this.passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(allErrors);
                }

                // check if user exists
                var user = await loginRepository.GetAsync(existingUser => existingUser.Username == loginRequest.Username);
                if (user != null)
                    return Conflict("Username already exists");

                // Hash the password
                var (passSalt, passHash) = passwordHasher.CreatePasswordHash(loginRequest.Password);
                user = new LoginUser
                {
                    Id = Guid.NewGuid(),
                    Username = loginRequest.Username,
                    PasswordHash = passHash,
                    PasswordSalt = passSalt
                };

                // Adds the user in the Users Microservice
                // If it fails, an exception will be thrown and the user won't be saved
                var createUserDto = new CreateUserDto(user.Id, user.Username);
                await userClient.CreateUserAsync(createUserDto);

                await loginRepository.CreateAsync(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(allErrors);
            }

            // check if user exists
            var user = await loginRepository.GetAsync(existingUser => existingUser.Username == loginRequest.Username);
            if (user == null)
                return Unauthorized("Username or Password are Invalid");

            // Verify hashed pass
            var isUserVerified = passwordHasher.VerifyPasswordHash(loginRequest.Password, user.PasswordHash, user.PasswordSalt);
            if (!isUserVerified)
                return Unauthorized("Username or Password are Invalid");

            //Create token
            var token = jwtTokenGenerator.GenerateToken(user);

            LoginResult result = new(user.Id, token, "Login Successful", true);
            return Ok(result);
        }
    }
}
