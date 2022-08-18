using TalkBack.Login.Service.Dtos;

namespace TalkBack.Login.Service.Clients
{
    public class UserClient
    {
        private readonly HttpClient httpClient;

        public UserClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // Retrieve the users from Users.Service
        // Consumer is not expected to modify the collection in any way, hence IReadOnlyCollection
        //public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync()
        //{
        //    var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<UserDto>>("api/Users");
        //    return items;
        //}

        public async Task CreateUserAsync(CreateUserDto createUserDto)
        {
            await httpClient.PostAsJsonAsync("api/Users", createUserDto);
        }
    }
}
