using WordsTrainer.Core.Entities;

namespace WordsTrainer.Api.Abstractions
{
    public interface IJwtService
    {
        string Generate(AppUser user);
    }
}
