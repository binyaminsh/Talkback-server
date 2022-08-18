using System.ComponentModel.DataAnnotations;

namespace TalkBack.Login.Service.Dtos
{
    public record LoginRequest([Required] string Username, [Required] string Password);
    public record LoginResult(Guid Id, string? Token, string Message, bool Success);


    public record LoginUserDto(Guid Id, string Username);

    // Dto that represents a user in Users.Service
    public record UserDto(Guid Id, string DisplayName, int Rating);

    public record CreateUserDto(Guid Id, string DisplayName);
}
