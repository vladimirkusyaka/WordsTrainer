using System.Net.Http.Headers;
using System.Net.Http.Json;
using WordsTrainer.Api.Abstractions;

namespace WordsTrainer.Api.Services;

public sealed class BrevoPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrevoPasswordResetEmailSender> _logger;

    public BrevoPasswordResetEmailSender(
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger<BrevoPasswordResetEmailSender> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendResetPasswordEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Brevo:ApiKey"];
        var fromEmail = _configuration["Brevo:FromEmail"];
        var fromName = _configuration["Brevo:FromName"] ?? "WordsTrainer";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning("Brevo API key or FromEmail is not configured.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("api-key", apiKey);

        var payload = new
        {
            sender = new { name = fromName, email = fromEmail },
            to = new[] { new { email = toEmail } },
            subject = "Reset your WordsTrainer password",
            htmlContent =
                $"""
                <p>We received a request to reset your password.</p>
                <p><a href="{resetLink}">Click here to reset password</a></p>
                <p>This link is valid for 30 minutes.</p>
                """
        };

        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Brevo send failed. Status={Status}. Body={Body}", response.StatusCode, body);
        }
    }
}