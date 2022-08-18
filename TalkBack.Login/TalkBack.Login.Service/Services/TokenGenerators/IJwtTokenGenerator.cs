using TalkBack.Login.Service.Entities;

namespace TalkBack.Login.Service.Services.TokenGenerators
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(LoginUser loginUser);
    }
}