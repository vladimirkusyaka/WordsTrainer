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
        private int _step = 1;

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

        public int Step
        {
            get => _step;
            set
            {
                if (_step == value) return;
                _step = value;
                StepPropertyChanged();
            }
        }

        public bool IsStep1 => Step == 1;
        public bool IsStep2 => Step == 2;
        public bool IsStep3 => Step == 3;

        public string StepTitle => Step switch
        {
            1 => "Create account",
            2 => "Your learning setup",
            3 => "Confirm details",
            _ => "Create account"
        };

        public string StepSubtitle => Step switch
        {
            1 => "Enter your login details",
            2 => "Choose your languages and level",
            3 => "Review and start learning",
            _ => ""
        };

        public Color Step1Color => Step == 1
                ? (Color)Application.Current!.Resources["WtPrimary"]
                : Color.FromArgb("#DDE3EF");
        public Color Step2Color => Step == 2
                ? (Color)Application.Current!.Resources["WtPrimary"]
                : Color.FromArgb("#DDE3EF");
        public Color Step3Color => Step == 3
                ? (Color)Application.Current!.Resources["WtPrimary"]
                : Color.FromArgb("#DDE3EF");

        public ICommand NextStepCommand => new Command(() =>
        {
            if (Step < 3)
                Step++;
        });

        public ICommand PreviousStepCommand => new Command(() =>
        {
            if (Step > 1)
                Step--;
        });

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

        private void StepPropertyChanged()
        {
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            OnPropertyChanged(nameof(IsStep3));
            OnPropertyChanged(nameof(StepTitle));
            OnPropertyChanged(nameof(StepSubtitle));
            OnPropertyChanged(nameof(Step1Color));
            OnPropertyChanged(nameof(Step2Color));
            OnPropertyChanged(nameof(Step3Color));
        }
    }
}
