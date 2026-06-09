using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WordsTrainer.Web.Components;
using WordsTrainer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "WordsTrainer.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<PasswordResetApiClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Api:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Configuration key 'Api:BaseUrl' is required.");

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

builder.Services.AddHttpClient<AdminApiClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Api:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Configuration key 'Api:BaseUrl' is required.");

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/admin/sign-in", async (
    HttpContext context,
    IConfiguration configuration,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("AdminLogin");
    var returnUrl = string.Empty;

    try
    {
        var configuredPassword = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(configuredPassword))
            return Results.Redirect("/admin/login?error=not-configured");

        var form = await context.Request.ReadFormAsync();
        var password = form["password"].ToString();
        returnUrl = form["returnUrl"].ToString();

        if (!string.Equals(password, configuredPassword, StringComparison.Ordinal))
            return Results.Redirect(BuildLoginUrl(returnUrl, "invalid"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return Results.Redirect(GetLocalReturnUrl(returnUrl) ?? "/admin/errors");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Admin login failed.");
        return Results.Redirect(BuildLoginUrl(returnUrl, "server"));
    }
}).DisableAntiforgery();

app.MapGet("/admin/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/admin/login");
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string BuildLoginUrl(string? returnUrl, string error)
{
    var url = $"/admin/login?error={Uri.EscapeDataString(error)}";

    if (!string.IsNullOrWhiteSpace(returnUrl))
        url += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

    return url;
}

static string? GetLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return null;

    return returnUrl.StartsWith('/') && !returnUrl.StartsWith("//", StringComparison.Ordinal)
        ? returnUrl
        : null;
}
