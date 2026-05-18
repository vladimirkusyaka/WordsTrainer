using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WordsTrainer.Contracts.LanguageLevels;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Api.Controllers
{
    [ApiController]
    [Route("api/language-levels")]
    public class LanguageLevelsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LanguageLevelsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<LanguageLevelResponse>>> Get()
        {
            var levels = await _db.LanguageLevels
                .Where(x => x.IsActive)
                .OrderBy(x => x.Order)
                .Select(x => new LanguageLevelResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Order = x.Order
                })
                .ToListAsync();

            return Ok(levels);
        }
    }
}
