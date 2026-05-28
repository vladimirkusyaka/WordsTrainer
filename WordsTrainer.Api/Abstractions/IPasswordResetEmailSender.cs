namespace WordsTrainer.Api.Abstractions;

public interface IPasswordResetEmailSender
{
    Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
}
