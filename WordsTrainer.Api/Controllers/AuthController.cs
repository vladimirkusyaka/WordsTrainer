using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordsTrainer.Api.Abstractions;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;
using LoginRequest = WordsTrainer.Contracts.Auth.LoginRequest;
using RegisterRequest = WordsTrainer.Contracts.Auth.RegisterRequest;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(AppDbContext db, IJwtService jwt, IPasswordHasher passwordHasher)
    {
        _db = db;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(x => x.Email == req.Email))
            return BadRequest("User already exists");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = req.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(req.Password),
            NativeLanguageId = req.NativeLanguageId,
            TargetLanguageId = req.TargetLanguageId,
            LanguageLevelId = req.LanguageLevelId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.Generate(user);

        return new AuthResponse { AccessToken = token };
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Email == req.Email);

        if (user == null || !_passwordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var token = _jwt.Generate(user);

        return new AuthResponse { AccessToken = token };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        var userId = Guid.Parse(userIdClaim);

        var user = await _db.Users
            .Include(x => x.NativeLanguage)
            .Include(x => x.TargetLanguage)
            .Include(x => x.LanguageLevel)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return Unauthorized();

        return Ok(new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            NativeLanguageCode = user.NativeLanguage.Code,
            TargetLanguageCode = user.TargetLanguage.Code,
            LanguageLevelId = user.LanguageLevelId,
            LanguageLevelCode = user.LanguageLevel.Code,
            LanguageLevelName = user.LanguageLevel.Name
        });
    }
}
