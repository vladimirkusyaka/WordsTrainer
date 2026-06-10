using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WordsTrainer.Api.Abstractions;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Contracts.Common;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_CreatesUserWithNormalizedEmail_AndReturnsToken()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var jwt = new FakeJwtService();
        var controller = CreateController(db, jwt: jwt);

        var result = await controller.Register(new RegisterRequest
        {
            Email = " USER@Example.COM ",
            Password = "Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });

        var response = Assert.IsType<AuthResponse>(result.Value);
        Assert.Equal("token:user@example.com", response.AccessToken);

        var user = await db.Users.SingleAsync();
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hashed:Password123", user.PasswordHash);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hash",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "USER@example.com",
            Password = "Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal("register.email.exists", error.Code);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Register_WhenNativeAndTargetLanguagesAreSame_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var controller = CreateController(db);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.NativeLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal("register.languages.same", error.Code);
        Assert.Equal("Native and target languages must be different.", error.Message);
        Assert.Equal(0, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed:Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db, jwt: new FakeJwtService());

        var result = await controller.Login(new LoginRequest
        {
            Email = " USER@example.com ",
            Password = "Password123"
        });

        var response = Assert.IsType<AuthResponse>(result.Value);
        Assert.Equal("token:user@example.com", response.AccessToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed:Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Login(new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrong"
        });

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(unauthorized.Value);
        Assert.Equal("login.failed", error.Code);
    }

    [Fact]
    public async Task ForgotPassword_ForUnknownEmail_ReturnsGenericMessage_AndCreatesNoToken()
    {
        await using var db = CreateDb();
        var emailSender = new CapturingPasswordResetEmailSender();
        var controller = CreateController(db, emailSender: emailSender);

        var result = await controller.ForgotPassword(new ForgotPasswordRequest
        {
            Email = "missing@example.com"
        });

        var response = AssertOk<AuthMessageResponse>(result);
        Assert.Equal("forgot.sent", response.Code);
        Assert.Contains("If an account exists", response.Message);
        Assert.Empty(emailSender.SentLinks);
        Assert.Equal(0, await db.PasswordResetTokens.CountAsync());
    }

    [Fact]
    public async Task ForgotPassword_ForExistingEmail_CreatesToken_AndSendsResetLink()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var user = AddUser(db, seed, "user@example.com");
        await db.SaveChangesAsync();

        var emailSender = new CapturingPasswordResetEmailSender();
        var controller = CreateController(db, emailSender: emailSender);

        var result = await controller.ForgotPassword(new ForgotPasswordRequest
        {
            Email = "USER@example.com"
        });

        AssertOk<AuthMessageResponse>(result);

        var token = await db.PasswordResetTokens.SingleAsync();
        Assert.Equal(user.Id, token.UserId);
        Assert.Null(token.UsedAtUtc);
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);

        var sent = Assert.Single(emailSender.SentLinks);
        Assert.Equal("user@example.com", sent.ToEmail);
        Assert.StartsWith("https://example.com/reset-password?token=", sent.ResetLink);
    }

    [Fact]
    public async Task ForgotPassword_WhenActiveTokenIsTooFresh_DoesNotCreateOrSendAnotherToken()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var user = AddUser(db, seed, "user@example.com");
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = new string('A', 64),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(29)
        });
        await db.SaveChangesAsync();

        var emailSender = new CapturingPasswordResetEmailSender();
        var controller = CreateController(db, emailSender: emailSender);

        await controller.ForgotPassword(new ForgotPasswordRequest
        {
            Email = "user@example.com"
        });

        Assert.Equal(1, await db.PasswordResetTokens.CountAsync());
        Assert.Empty(emailSender.SentLinks);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesPassword_AndMarksTokenUsed()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var user = AddUser(db, seed, "user@example.com");
        await db.SaveChangesAsync();

        var emailSender = new CapturingPasswordResetEmailSender();
        var controller = CreateController(db, emailSender: emailSender);
        await controller.ForgotPassword(new ForgotPasswordRequest { Email = user.Email });

        var resetLink = Assert.Single(emailSender.SentLinks).ResetLink;
        var rawToken = new Uri(resetLink).Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', 2))
            .Single(x => x[0] == "token")[1];

        var result = await controller.ResetPassword(new ResetPasswordRequest
        {
            Token = Uri.UnescapeDataString(rawToken),
            Password = "NewPassword123",
            ConfirmPassword = "NewPassword123"
        });

        var response = AssertOk<AuthMessageResponse>(result);
        Assert.Contains("Password changed successfully", response.Message);

        var reloadedUser = await db.Users.SingleAsync(x => x.Id == user.Id);
        Assert.Equal("hashed:NewPassword123", reloadedUser.PasswordHash);

        var token = await db.PasswordResetTokens.SingleAsync();
        Assert.NotNull(token.UsedAtUtc);
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ReturnsBadRequest()
    {
        await using var db = CreateDb();
        var seed = SeedReferenceData(db);
        var user = AddUser(db, seed, "user@example.com");
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = new string('B', 64),
            CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(-1)
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.ResetPassword(new ResetPasswordRequest
        {
            Token = "expired",
            Password = "NewPassword123",
            ConfirmPassword = "NewPassword123"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal("reset.token.invalid", error.Code);

        var reloadedUser = await db.Users.SingleAsync(x => x.Id == user.Id);
        Assert.Equal("hashed:Password123", reloadedUser.PasswordHash);
    }

    private static AuthController CreateController(
        AppDbContext db,
        IJwtService? jwt = null,
        IPasswordHasher? passwordHasher = null,
        IPasswordResetEmailSender? emailSender = null,
        IConfiguration? configuration = null)
    {
        var controller = new AuthController(
            db,
            jwt ?? new FakeJwtService(),
            passwordHasher ?? new FakePasswordHasher(),
            emailSender ?? new CapturingPasswordResetEmailSender(),
            configuration ?? CreateConfiguration(),
            NullLogger<AuthController>.Instance);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private static T AssertOk<T>(ActionResult<T> result)
    {
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsType<T>(ok.Value);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PasswordReset:ResetUrl"] = "https://example.com/reset-password"
            })
            .Build();
    }

    private static SeedContext SeedReferenceData(AppDbContext db)
    {
        var native = new Language
        {
            Id = Guid.NewGuid(),
            Code = "ru",
            Name = "Russian",
            NativeName = "Russkiy"
        };

        var target = new Language
        {
            Id = Guid.NewGuid(),
            Code = "de",
            Name = "German",
            NativeName = "Deutsch"
        };

        var level = new LanguageLevel
        {
            Id = Guid.NewGuid(),
            Code = "A1",
            Name = "Beginner",
            Order = 1
        };

        db.Languages.AddRange(native, target);
        db.LanguageLevels.Add(level);

        return new SeedContext(native, target, level);
    }

    private static AppUser AddUser(AppDbContext db, SeedContext seed, string email)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed:Password123",
            NativeLanguageId = seed.NativeLanguage.Id,
            TargetLanguageId = seed.TargetLanguage.Id,
            LanguageLevelId = seed.Level.Id
        };

        db.Users.Add(user);

        return user;
    }

    private sealed class FakeJwtService : IJwtService
    {
        public string Generate(AppUser user)
        {
            return $"token:{user.Email}";
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"hashed:{password}";
        }

        public bool Verify(string password, string hash)
        {
            return hash == Hash(password);
        }
    }

    private sealed class CapturingPasswordResetEmailSender : IPasswordResetEmailSender
    {
        public List<(string ToEmail, string ResetLink)> SentLinks { get; } = [];

        public Task SendResetPasswordEmailAsync(
            string toEmail,
            string resetLink,
            CancellationToken cancellationToken = default)
        {
            SentLinks.Add((toEmail, resetLink));
            return Task.CompletedTask;
        }
    }

    private sealed record SeedContext(
        Language NativeLanguage,
        Language TargetLanguage,
        LanguageLevel Level);
}
