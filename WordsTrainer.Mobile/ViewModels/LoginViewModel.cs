using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private readonly TokenStorage _tokenStorage;

        private string _email = "";
        private string _password = "";
        private string _errorMessage = "";
        private bool _isBusy;

        public LoginViewModel(ApiClient apiClient, TokenStorage tokenStorage)
        {
            _apiClient = apiClient;
            _tokenStorage = tokenStorage;

            LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
            GoToRegisterCommand = new Command(async () => await GoToRegisterAsync(), () => !IsBusy);
            BackCommand = new Command(async () => await BackAsync(), () => !IsBusy);
        }


        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetField(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetField(ref _isBusy, value))
                {
                    ((Command)LoginCommand).ChangeCanExecute();
                    ((Command)GoToRegisterCommand).ChangeCanExecute();
                }
            }
        }

        public ICommand GoToRegisterCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите email и пароль.";
                return;
            }

            try
            {
                IsBusy = true;

                var response = await _apiClient.LoginAsync(new LoginRequest
                {
                    Email = Email.Trim(),
                    Password = Password
                });

                if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
                {
                    ErrorMessage = "Неверный email или пароль.";
                    return;
                }

                await _tokenStorage.SaveAccessTokenAsync(response.AccessToken);

                var trainingPage = Application.Current!.Handler!.MauiContext!.Services
    .GetRequiredService<TrainingPage>();

                await trainingPage.InitializeAsync();

                var window = Application.Current!.Windows[0];
                window.Page = new NavigationPage(trainingPage);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка входа: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }

        private async Task GoToRegisterAsync()
        {
            var registerPage = Application.Current!.Handler!.MauiContext!.Services
                .GetRequiredService<RegisterPage>();

            Application.Current.Windows[0].Page = new NavigationPage(registerPage);

            await Task.CompletedTask;
        }

        private async Task BackAsync()
        {
            var welcomePage = Application.Current!.Handler!.MauiContext!.Services
                .GetRequiredService<WelcomePage>();

            Application.Current.Windows[0].Page = new NavigationPage(welcomePage);

            await Task.CompletedTask;

        }
    }
}
