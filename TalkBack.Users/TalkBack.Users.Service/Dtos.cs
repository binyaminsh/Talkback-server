using System.ComponentModel.DataAnnotations;

namespace TalkBack.Users.Service.Dtos
{
    public record UserDto (Guid Id, string DisplayName, int ChessRating);

    public record CreateUserDto ([Required] Guid Id, [Required] string DisplayName);

    public record UpdateUserDto ([Required] string DisplayName);
}
