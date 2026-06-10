using System.Net.Http.Json;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Contracts.Common;

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

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<AuthMessageResponse>(cancellationToken);
                return (true, content?.Message ?? "Password changed successfully.");
            }

            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken);

            return (false, error?.Message ?? "Unable to reset password. Please try again.");
        }
        catch
        {
            return (false, "Unable to reset password. Please check your connection and try again.");
        }
    }
}
