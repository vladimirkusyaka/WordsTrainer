using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Contracts.Common;
using WordsTrainer.Contracts.LanguageLevels;
using WordsTrainer.Contracts.Languages;
using WordsTrainer.Contracts.Training;

namespace WordsTrainer.Mobile.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly TokenStorage _tokenStorage;

        public ApiClient(HttpClient http, TokenStorage tokenStorage)
        {
            _http = http;
            _tokenStorage = tokenStorage;
        }

        public async Task<ApiResult<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/register", request);

            return await ReadResultAsync<AuthResponse>(response);
        }

        public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", request);

            return await ReadResultAsync<AuthResponse>(response);
        }

        public async Task<ApiResult<AuthMessageResponse>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/forgot-password", request);

            return await ReadResultAsync<AuthMessageResponse>(
                response,
                tooManyRequestsCode: "forgot.too.many",
                tooManyRequestsMessage: "Too many password reset requests. Please try again later.");
        }

        public async Task<ApiResult<AuthMessageResponse>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/reset-password", request);

            return await ReadResultAsync<AuthMessageResponse>(response);
        }

        public async Task<ApiResult<CurrentUserResponse>> GetMeAsync()
        {
            await AddBearerAsync();

            var response = await _http.GetAsync("/api/auth/me");

            return await ReadResultAsync<CurrentUserResponse>(response);
        }

        public async Task<ApiResult<TrainingNextResponse>> GetNextAsync()
        {
            await AddBearerAsync();

            var response = await _http.GetAsync("/api/training/next");

            return await ReadResultAsync<TrainingNextResponse>(response);
        }

        public async Task<ApiResult<SubmitTrainingAnswerResponse>> SubmitAnswerAsync(
            SubmitTrainingAnswerRequest request)
        {
            await AddBearerAsync();

            var response = await _http.PostAsJsonAsync(
                "/api/training/answer",
                request);

            return await ReadResultAsync<SubmitTrainingAnswerResponse>(response);
        }

        public async Task<ApiResult<TrainingStatsResponse>> GetStatsAsync()
        {
            await AddBearerAsync();

            var response = await _http.GetAsync("/api/training/stats");

            return await ReadResultAsync<TrainingStatsResponse>(response);
        }

        public async Task<ApiResult<TrainingExplanationResponse>> GetExplanationAsync(Guid attemptId)
        {
            await AddBearerAsync();

            var response = await _http.GetAsync($"/api/training/explanation/attempt/{attemptId}");

            return await ReadResultAsync<TrainingExplanationResponse>(response);
        }

        public async Task<ApiResult<List<LanguageResponse>>> GetLanguagesAsync()
        {
            var response = await _http.GetAsync("/api/languages");

            return await ReadResultAsync<List<LanguageResponse>>(response);
        }

        public async Task<ApiResult<List<LanguageLevelResponse>>> GetLanguageLevelsAsync()
        {
            var response = await _http.GetAsync("/api/language-levels");

            return await ReadResultAsync<List<LanguageLevelResponse>>(response);
        }

        private async Task AddBearerAsync()
        {
            var token = await _tokenStorage.GetAccessTokenAsync();

            _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private static async Task<ApiResult<T>> ReadResultAsync<T>(
            HttpResponseMessage response,
            string? tooManyRequestsCode = null,
            string? tooManyRequestsMessage = null)
        {
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>();
                return ApiResult<T>.Success(value);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return ApiResult<T>.Failure(
                    tooManyRequestsCode ?? "too.many.requests",
                    tooManyRequestsMessage ?? "Too many requests. Please try again later.",
                    response.StatusCode);
            }

            var error = await ReadErrorAsync(response);

            return ApiResult<T>.Failure(
                error?.Code ?? GetDefaultErrorCode(response.StatusCode),
                error?.Message ?? response.ReasonPhrase ?? "Request failed.",
                response.StatusCode);
        }

        private static async Task<ApiErrorResponse?> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            }
            catch
            {
                return null;
            }
        }

        private static string GetDefaultErrorCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.Unauthorized => "auth.unauthorized",
                HttpStatusCode.NotFound => "not.found",
                _ => "api.request.failed"
            };
        }
    }
}
