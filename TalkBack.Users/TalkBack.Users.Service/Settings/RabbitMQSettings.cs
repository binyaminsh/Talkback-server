namespace TalkBack.Users.Service.Settings
{
    public class RabbitMQSettings
    {
        // "init" because no one should "set" this after deserialize from config
        public string Host { get; init; } = null!;
    }
}
