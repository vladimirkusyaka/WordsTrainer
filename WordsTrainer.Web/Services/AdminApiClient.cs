using System.Net.Http.Json;
using WordsTrainer.Contracts.Admin;

namespace WordsTrainer.Web.Services
{
    public sealed class AdminApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AdminApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ErrorLogListResponse?> GetErrorsAsync(
            int page,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, BuildErrorsUrl(page, pageSize, search));
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ErrorLogListResponse>(cancellationToken);
        }

        public async Task<ErrorLogDetailResponse?> GetErrorAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, $"api/admin/errors/{id}");
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ErrorLogDetailResponse>(cancellationToken);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string requestUri)
        {
            var apiKey = _configuration["Admin:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Configuration key 'Admin:ApiKey' is required.");

            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("X-Admin-Key", apiKey);
            return request;
        }

        private static string BuildErrorsUrl(int page, int pageSize, string? search)
        {
            var url = $"api/admin/errors?page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";

            return url;
        }
    }
}
