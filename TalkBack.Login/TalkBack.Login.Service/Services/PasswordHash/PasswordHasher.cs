using System.Security.Cryptography;
using System.Text;

namespace TalkBack.Login.Service.Services.PasswordHash
{
    public class PasswordHasher : IPasswordHasher
    {
        public (byte[] passSalt, byte[] passHash) CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                byte[] passowrdSalt = hmac.Key;
                byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return (passowrdSalt, passwordHash);
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passowrdSalt)
        {
            using (var hmac = new HMACSHA512(passowrdSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
