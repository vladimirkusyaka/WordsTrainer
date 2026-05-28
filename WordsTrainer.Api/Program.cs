using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WordsTrainer.Api.Abstractions;
using WordsTrainer.Api.Security;
using WordsTrainer.Api.Services;
using WordsTrainer.Infrastructure.Data;
using WordsTrainer.Infrastructure.Seed;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    //    options.UseSqlServer(
    //        builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddHttpClient<BrevoPasswordResetEmailSender>();
builder.Services.AddScoped<IPasswordResetEmailSender, BrevoPasswordResetEmailSender>(); 
builder.Services.AddScoped<TrainingService>();
builder.Services.AddScoped<SeedImportService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введите токен так: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();


var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup.Configuration");

var smtpHost = app.Configuration["Smtp:Host"];
var smtpPort = app.Configuration["Smtp:Port"];
var smtpUseSsl = app.Configuration["Smtp:UseSsl"];
var smtpFrom = app.Configuration["Smtp:FromEmail"];
var smtpUsername = app.Configuration["Smtp:Username"];
var smtpPassword = app.Configuration["Smtp:Password"];
var resetUrl = app.Configuration["PasswordReset:ResetUrl"];

startupLogger.LogInformation(
    "Password reset config: ResetUrlConfigured={ResetUrlConfigured}; SMTP HostConfigured={HostConfigured}, FromConfigured={FromConfigured}, UsernameConfigured={UsernameConfigured}, PasswordConfigured={PasswordConfigured}, Port={Port}, UseSsl={UseSsl}",
    !string.IsNullOrWhiteSpace(resetUrl),
    !string.IsNullOrWhiteSpace(smtpHost),
    !string.IsNullOrWhiteSpace(smtpFrom),
    !string.IsNullOrWhiteSpace(smtpUsername),
    !string.IsNullOrWhiteSpace(smtpPassword),
    string.IsNullOrWhiteSpace(smtpPort) ? "(default)" : smtpPort,
    string.IsNullOrWhiteSpace(smtpUseSsl) ? "(default true)" : smtpUseSsl);

/**/
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await DbSeeder.SeedAsync(db);

    var importer = scope.ServiceProvider.GetRequiredService<SeedImportService>();

    var seedDirectoryPath = Path.Combine(
        app.Environment.ContentRootPath,
        "SeedData");

    await importer.ImportDirectoryAsync(seedDirectoryPath);
}


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
