using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkBack.Login.Contracts
{
    public record UserCreated(Guid Id, string Username);
}
