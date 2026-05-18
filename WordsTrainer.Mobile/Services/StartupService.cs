using System;
using System.Collections.Generic;
using System.Text;
using WordsTrainer.Mobile.Pages;

namespace WordsTrainer.Mobile.Services
{
    public class StartupService
    {
        private readonly TokenStorage _tokenStorage;
        private readonly ApiClient _apiClient;

        private readonly LoginPage _loginPage;
        private readonly TrainingPage _trainingPage;

        public StartupService(
            TokenStorage tokenStorage,
            ApiClient apiClient,
            LoginPage loginPage,
            TrainingPage trainingPage)
        {
            _tokenStorage = tokenStorage;
            _apiClient = apiClient;

            _loginPage = loginPage;
            _trainingPage = trainingPage;

//            _tokenStorage.Clear();
        }

        public async Task<Page> GetStartupPageAsync()
        {
            var token = await _tokenStorage.GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return new NavigationPage(_loginPage);

            var me = await _apiClient.GetMeAsync();

            if (me == null)
            {
                _tokenStorage.Clear();
                return new NavigationPage(_loginPage);
            }

            await _trainingPage.InitializeAsync();
            return new NavigationPage(_trainingPage);
        }
    }
}
