
using WordsTrainer.Api.Abstractions;

namespace WordsTrainer.Api.Services
{
    public class AuthService
    {
        private readonly IPasswordHasher _hasher;

        public AuthService(IPasswordHasher hasher)
        {
            _hasher = hasher;
        }

        public void Example(string password)
        {
            var hash = _hasher.Hash(password);
        }
    }
}
