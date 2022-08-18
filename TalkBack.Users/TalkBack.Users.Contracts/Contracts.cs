using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkBack.Users.Contracts
{
    // TODO: handle all cases
    public record UserCreated(Guid Id, string DisplayName, int ChessRating);
    public record UserUpdated(Guid Id, string DisplayName, int Rating);
    public record UserDeleted(Guid Id);
}