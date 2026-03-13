namespace VotingSystemBackend.Services
{
    public interface IEncryptionService
    {
        string Hash(string value);
        bool Verify(string value, string hash);
        string GenerateToken(int length = 32);
    }
}
