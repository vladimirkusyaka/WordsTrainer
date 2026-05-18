using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Contracts.LanguageLevels;
using WordsTrainer.Contracts.Languages;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly TokenStorage _tokenStorage;

        private string _email = "";
        private string _password = "";
        private string _confirmPassword = "";
        private string _errorMessage = "";
        private bool _isBusy;

        private LanguageResponse? _selectedNativeLanguage;
        private LanguageResponse? _selectedTargetLanguage;
        private LanguageLevelResponse? _selectedLevel;

        public List<LanguageLevelResponse> Levels { get; private set; } = [];


        public LanguageLevelResponse? SelectedLevel
        {
            get => _selectedLevel;
            set => SetField(ref _selectedLevel, value);
        }

        public RegisterViewModel(ApiClient apiClient, TokenStorage tokenStorage)
        {
            _apiClient = apiClient;
            _tokenStorage = tokenStorage;

            RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
            GoToLoginCommand = new Command(async () => await GoToLoginAsync(), () => !IsBusy);
        }

        public string Email { get => _email; set => SetField(ref _email, value); }
        public string Password { get => _password; set => SetField(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetField(ref _confirmPassword, value); }
        public string ErrorMessage { get => _errorMessage; set => SetField(ref _errorMessage, value); }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetField(ref _isBusy, value))
                {
                    ((Command)RegisterCommand).ChangeCanExecute();
                    ((Command)GoToLoginCommand).ChangeCanExecute();
                }
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public LanguageResponse? SelectedNativeLanguage
        {
            get => _selectedNativeLanguage;
            set => SetField(ref _selectedNativeLanguage, value);
        }

        public LanguageResponse? SelectedTargetLanguage
        {
            get => _selectedTargetLanguage;
            set => SetField(ref _selectedTargetLanguage, value);
        }

        public List<LanguageResponse> Languages { get; private set; } = [];

        public async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = "";

                Languages = await _apiClient.GetLanguagesAsync();
                OnPropertyChanged(nameof(Languages));

                SelectedNativeLanguage =
                    Languages.FirstOrDefault(x => x.Code == "ru") ??
                    Languages.FirstOrDefault();

                SelectedTargetLanguage =
                    Languages.FirstOrDefault(x => x.Code == "de") ??
                    Languages.Skip(1).FirstOrDefault() ??
                    Languages.FirstOrDefault();

                Levels = await _apiClient.GetLanguageLevelsAsync();
                OnPropertyChanged(nameof(Levels));

                SelectedLevel =
                    Levels.FirstOrDefault(x => x.Code == "A1") ??
                    Levels.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не удалось загрузить языки: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Заполните все поля.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return;
            }

            if (SelectedNativeLanguage == null || SelectedTargetLanguage == null)
            {
                ErrorMessage = "Выберите языки.";
                return;
            }

            if (SelectedNativeLanguage.Id == SelectedTargetLanguage.Id)
            {
                ErrorMessage = "Родной и изучаемый язык должны отличаться.";
                return;
            }

            if (SelectedLevel == null)
            {
                ErrorMessage = "Выберите уровень.";
                return;
            }

            try
            {
                IsBusy = true;

                var response = await _apiClient.RegisterAsync(new RegisterRequest
                {
                    Email = Email.Trim(),
                    Password = Password,
                    NativeLanguageId = SelectedNativeLanguage.Id,
                    TargetLanguageId = SelectedTargetLanguage.Id,
                    LanguageLevelId = SelectedLevel.Id
                });

                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                {
                    ErrorMessage = "Не удалось зарегистрироваться.";
                    return;
                }

                await _tokenStorage.SaveAccessTokenAsync(response.AccessToken);

                var trainingPage = Application.Current!.Handler!.MauiContext!.Services
                    .GetRequiredService<TrainingPage>();

                Application.Current.Windows[0].Page = new NavigationPage(trainingPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка регистрации: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToLoginAsync()
        {
            var loginPage = Application.Current!.Handler!.MauiContext!.Services
                .GetRequiredService<LoginPage>();

            Application.Current.Windows[0].Page = new NavigationPage(loginPage);

            await Task.CompletedTask;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
