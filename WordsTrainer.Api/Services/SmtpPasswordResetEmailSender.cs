using System.Net;
using System.Net.Mail;
using WordsTrainer.Api.Abstractions;

namespace WordsTrainer.Api.Services;

public sealed class SmtpPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpPasswordResetEmailSender> _logger;

    public SmtpPasswordResetEmailSender(
        IConfiguration configuration,
        ILogger<SmtpPasswordResetEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        var from = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "WordsTrainer";
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var useSsl = !bool.TryParse(_configuration["Smtp:UseSsl"], out var parsedUseSsl) || parsedUseSsl;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("SMTP is not configured. Password reset link for {Email}: {Link}", toEmail, resetLink);
            return;
        }

        var subject = "Reset your WordsTrainer password";
        var body =
            "We received a request to reset your WordsTrainer password.\n\n" +
            $"Use this link to set a new password:\n{resetLink}\n\n" +
            "This link expires in 30 minutes.\n\n" +
            "If you did not request a password reset, you can safely ignore this email.";

        using var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            Body = body
        };

        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password ?? string.Empty);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
