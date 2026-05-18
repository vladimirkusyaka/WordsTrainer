using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using WordsTrainer.Contracts.Auth;
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

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/register", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<CurrentUserResponse?> GetMeAsync()
        {
            await AddBearerAsync();

            var response = await _http.GetAsync("/api/auth/me");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }

        public async Task<TrainingNextResponse?> GetNextAsync()
        {
            await AddBearerAsync();

            return await _http.GetFromJsonAsync<TrainingNextResponse>(
                "/api/training/next");
        }

        public async Task<SubmitTrainingAnswerResponse?> SubmitAnswerAsync(
            SubmitTrainingAnswerRequest request)
        {
            await AddBearerAsync();

            var response = await _http.PostAsJsonAsync(
                "/api/training/answer",
                request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<SubmitTrainingAnswerResponse>();
        }

        public async Task<TrainingStatsResponse?> GetStatsAsync()
        {
            await AddBearerAsync();

            return await _http.GetFromJsonAsync<TrainingStatsResponse>(
                "/api/training/stats");
        }

        public async Task<TrainingExplanationResponse?> GetExplanationAsync(Guid attemptId)
        {
            await AddBearerAsync();

            return await _http.GetFromJsonAsync<TrainingExplanationResponse>(
                $"/api/training/explanation/attempt/{attemptId}");
        }

        public async Task<List<LanguageResponse>> GetLanguagesAsync()
        {
            var response = await _http.GetAsync("/api/languages");

            if (!response.IsSuccessStatusCode)
                return [];

            var languages = await response.Content.ReadFromJsonAsync<List<LanguageResponse>>();

            return languages ?? [];
        }

        public async Task<List<LanguageLevelResponse>> GetLanguageLevelsAsync()
        {
            var response = await _http.GetAsync("/api/language-levels");

            if (!response.IsSuccessStatusCode)
                return [];

            return await response.Content
                .ReadFromJsonAsync<List<LanguageLevelResponse>>() ?? [];
        }

        private async Task AddBearerAsync()
        {
            var token = await _tokenStorage.GetAccessTokenAsync();

            _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
