using System.Net.Http.Json;
using WordsTrainer.Contracts.Auth;

namespace WordsTrainer.Web.Services;

public sealed class PasswordResetApiClient
{
    private readonly HttpClient _httpClient;

    public PasswordResetApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/auth/reset-password",
                request,
                cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<AuthMessageResponse>(cancellationToken);

            if (response.IsSuccessStatusCode)
                return (true, content?.Message ?? "Password changed successfully.");

            return (false, content?.Message ?? "Unable to reset password. Please try again.");
        }
        catch
        {
            return (false, "Unable to reset password. Please check your connection and try again.");
        }
    }
}
