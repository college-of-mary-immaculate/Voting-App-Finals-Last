using System.Security.Cryptography;
using System.Text;

namespace VotingSystemBackend.Services
{
    public class EncryptionService : IEncryptionService
    {
        public string Hash(string value)
        {
            if (value == null) return null;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verify(string value, string hash)
        {
            if (value == null || hash == null)
                return false;

            return Hash(value) == hash;
        }

        public string GenerateToken(int length = 32)
        {
            var tokenBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }
}
