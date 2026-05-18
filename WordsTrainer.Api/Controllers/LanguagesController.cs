using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordsTrainer.Contracts.Languages;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Api.Controllers
{
    [ApiController]
    [Route("api/languages")]
    public class LanguagesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LanguagesController(AppDbContext db)
        {
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<LanguageResponse>>> GetLanguages()
        {
            var languages = await _db.Languages
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LanguageResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    NativeName = x.NativeName
                }).ToListAsync();

            return Ok(languages);
        }
    }
}
