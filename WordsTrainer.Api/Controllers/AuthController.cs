using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
    private static readonly TimeSpan PasswordResetTokenTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan PasswordResetResendCooldown = TimeSpan.FromMinutes(3);

    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetEmailSender _passwordResetEmailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext db,
        IJwtService jwt,
        IPasswordHasher passwordHasher,
        IPasswordResetEmailSender passwordResetEmailSender,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _db = db;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
        _passwordResetEmailSender = passwordResetEmailSender;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var normalizedEmail = NormalizeEmail(req.Email);

        if (await _db.Users.AnyAsync(x => x.Email == normalizedEmail))
            return BadRequest("User already exists");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
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
        var normalizedEmail = NormalizeEmail(req.Email);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

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

    [HttpPost("forgot-password")]
    [EnableRateLimiting("forgot-password")]
    public async Task<ActionResult<AuthMessageResponse>> ForgotPassword(ForgotPasswordRequest request)
    {
        var response = new AuthMessageResponse
        {
            Message = "If an account exists for this email, a password reset link has been sent."
        };

        var normalizedEmail = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
            return Ok(response);

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
        if (user == null)
            return Ok(response);

        var now = DateTime.UtcNow;
        var activeToken = await _db.PasswordResetTokens
            .Where(x =>
                x.UserId == user.Id &&
                x.UsedAtUtc == null &&
                x.ExpiresAtUtc > now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        string rawToken;
        if (activeToken != null)
        {
            var sinceCreated = now - activeToken.CreatedAtUtc;
            if (sinceCreated < PasswordResetResendCooldown)
            {
                _logger.LogInformation(
                    "Password reset resend cooldown for user {UserId}. Elapsed {ElapsedSeconds}s.",
                    user.Id,
                    sinceCreated.TotalSeconds);

                return Ok(response);
            }

            rawToken = GenerateRawToken();
            activeToken.TokenHash = HashToken(rawToken);
            activeToken.CreatedAtUtc = now;
            activeToken.ExpiresAtUtc = now.Add(PasswordResetTokenTtl);
            activeToken.CreatedIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        else
        {
            rawToken = GenerateRawToken();
            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.Add(PasswordResetTokenTtl),
                CreatedIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _db.PasswordResetTokens.Add(resetToken);
        }

        await _db.SaveChangesAsync();

        var resetUrl = _configuration["PasswordReset:ResetUrl"];
        if (string.IsNullOrWhiteSpace(resetUrl))
        {
            _logger.LogWarning("PasswordReset:ResetUrl is not configured. Token generated for user {UserId}", user.Id);
            return Ok(response);
        }

        var separator = resetUrl.Contains('?') ? "&" : "?";
        var resetLink = $"{resetUrl}{separator}token={Uri.EscapeDataString(rawToken)}";

        try
        {
            await _passwordResetEmailSender.SendResetPasswordEmailAsync(user.Email, resetLink, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reset password email for user {UserId}", user.Id);
        }

        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthMessageResponse>> ResetPassword(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new AuthMessageResponse { Message = "Reset token is required." });

        if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
            return BadRequest(new AuthMessageResponse { Message = "Password and confirmation are required." });

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new AuthMessageResponse { Message = "Password confirmation does not match." });

        if (request.Password.Length < 8)
            return BadRequest(new AuthMessageResponse { Message = "Password must be at least 8 characters." });

        var tokenHash = HashToken(request.Token);
        var now = DateTime.UtcNow;

        var token = await _db.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.UsedAtUtc == null &&
                x.ExpiresAtUtc > now);

        if (token == null)
        {
            return BadRequest(new AuthMessageResponse
            {
                Message = "This reset link is invalid or expired. Request a new one."
            });
        }

        token.User.PasswordHash = _passwordHasher.Hash(request.Password);
        token.UsedAtUtc = now;

        var activeTokens = await _db.PasswordResetTokens
            .Where(x => x.UserId == token.UserId && x.UsedAtUtc == null && x.Id != token.Id)
            .ToListAsync();

        foreach (var item in activeTokens)
        {
            item.UsedAtUtc = now;
        }

        await _db.SaveChangesAsync();

        return Ok(new AuthMessageResponse
        {
            Message = "Password changed successfully. You can now sign in with your new password."
        });
    }

    private static string NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? string.Empty
            : email.Trim().ToLowerInvariant();
    }

    private static string GenerateRawToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(hash);
    }
}
