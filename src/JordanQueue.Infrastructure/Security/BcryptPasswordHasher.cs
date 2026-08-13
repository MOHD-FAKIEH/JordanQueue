using BCrypt.Net;
using JordanQueue.Application.Interfaces.Auth;

namespace JordanQueue.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        if (passwordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        // Legacy dev seed hashes (SHA256) — verify and allow login during migration
        return Infrastructure.Persistence.Seed.DatabaseSeeder.VerifyLegacyPassword(password, passwordHash);
    }
}
