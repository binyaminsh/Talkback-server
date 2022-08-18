namespace TalkBack.Login.Service.Services.PasswordHash
{
    public interface IPasswordHasher
    {
        public (byte[] passSalt, byte[] passHash) CreatePasswordHash(string password);
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passowrdSalt);
    }
}