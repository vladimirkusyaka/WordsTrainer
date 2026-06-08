using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordsTrainer.Contracts.Admin;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private const int MaxPageSize = 100;

        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public AdminController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpGet("errors")]
        public async Task<ActionResult<ErrorLogListResponse>> GetErrors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null)
        {
            if (!IsAuthorized())
                return Unauthorized();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var query = _db.ErrorLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(x =>
                    x.Message.Contains(value) ||
                    x.ExceptionType.Contains(value) ||
                    (x.RequestPath != null && x.RequestPath.Contains(value)) ||
                    (x.UserId != null && x.UserId.Contains(value)) ||
                    (x.TraceId != null && x.TraceId.Contains(value)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ErrorLogListItemResponse
                {
                    Id = x.Id,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Level = x.Level,
                    Message = x.Message,
                    ExceptionType = x.ExceptionType,
                    RequestMethod = x.RequestMethod,
                    RequestPath = x.RequestPath,
                    UserId = x.UserId,
                    TraceId = x.TraceId
                })
                .ToListAsync();

            return Ok(new ErrorLogListResponse
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            });
        }

        [HttpGet("errors/{id:guid}")]
        public async Task<ActionResult<ErrorLogDetailResponse>> GetError(Guid id)
        {
            if (!IsAuthorized())
                return Unauthorized();

            var item = await _db.ErrorLogs
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ErrorLogDetailResponse
                {
                    Id = x.Id,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Level = x.Level,
                    Message = x.Message,
                    ExceptionType = x.ExceptionType,
                    StackTrace = x.StackTrace,
                    RequestMethod = x.RequestMethod,
                    RequestPath = x.RequestPath,
                    QueryString = x.QueryString,
                    UserId = x.UserId,
                    RemoteIp = x.RemoteIp,
                    UserAgent = x.UserAgent,
                    TraceId = x.TraceId
                })
                .FirstOrDefaultAsync();

            return item == null ? NotFound() : Ok(item);
        }

        private bool IsAuthorized()
        {
            var expected = _configuration["Admin:ApiKey"];

            if (string.IsNullOrWhiteSpace(expected))
                return false;

            if (!Request.Headers.TryGetValue("X-Admin-Key", out var actual))
                return false;

            return string.Equals(actual.ToString(), expected, StringComparison.Ordinal);
        }
    }
}
