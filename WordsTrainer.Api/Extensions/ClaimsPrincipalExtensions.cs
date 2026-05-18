using System.Security.Claims;

namespace WordsTrainer.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
                throw new UnauthorizedAccessException("User id claim is missing.");

            return Guid.Parse(id);
        }
    }
}
