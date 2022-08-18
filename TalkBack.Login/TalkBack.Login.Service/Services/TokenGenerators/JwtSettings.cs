namespace TalkBack.Login.Service.Services.TokenGenerators
{
    public class JwtSettings
    {
        public string Secret { get; init; } = null!;
        public double ExpiryMinutes { get; init; }
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; } = null!;
    }
}
