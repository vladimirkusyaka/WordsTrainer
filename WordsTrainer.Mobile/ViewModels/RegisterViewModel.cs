using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WordsTrainer.Contracts.Auth;
using WordsTrainer.Contracts.LanguageLevels;
using WordsTrainer.Contracts.Languages;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels;

public class RegisterViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;

    private int _step = 1;
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _errorMessage = "";
    private bool _isBusy;

    private LanguageResponse? _selectedNativeLanguage;
    private LanguageResponse? _selectedTargetLanguage;
    private LanguageLevelResponse? _selectedLevel;

    public RegisterViewModel(ApiClient apiClient, TokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;

        NextStepCommand = new Command(NextStep, () => !IsBusy);
        PreviousStepCommand = new Command(PreviousStep, () => !IsBusy);
        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
        GoToLoginCommand = new Command(async () => await GoToLoginAsync(), () => !IsBusy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand NextStepCommand { get; }
    public ICommand PreviousStepCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public List<LanguageResponse> Languages { get; private set; } = [];
    public List<LanguageLevelResponse> Levels { get; private set; } = [];

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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetField(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetField(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
            {
                ((Command)NextStepCommand).ChangeCanExecute();
                ((Command)PreviousStepCommand).ChangeCanExecute();
                ((Command)RegisterCommand).ChangeCanExecute();
                ((Command)GoToLoginCommand).ChangeCanExecute();
            }
        }
    }

    public int Step
    {
        get => _step;
        set
        {
            if (_step == value)
                return;

            _step = value;
            StepPropertyChanged();
        }
    }

    public bool IsStep1 => Step == 1;
    public bool IsStep2 => Step == 2;
    public bool IsStep3 => Step == 3;

    public string StepTitle => Step switch
    {
        1 => "Create your account",
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

    public string StepCounterText => $"Step {Step} of 3";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanContinueStep =>
            !string.IsNullOrWhiteSpace(Email) &&
            Email.Contains('@') &&
            !string.IsNullOrWhiteSpace(Password) &&
            Password.Length >= 8 &&
            Password == ConfirmPassword;

    public Color Step1Color => Step >= 1
        ? (Color)Application.Current!.Resources["WtPrimary"]
        : Color.FromArgb("#DDE3EF");

    public Color Step2Color => Step >= 2
        ? (Color)Application.Current!.Resources["WtPrimary"]
        : Color.FromArgb("#DDE3EF");

    public Color Step3Color => Step >= 3
        ? (Color)Application.Current!.Resources["WtPrimary"]
        : Color.FromArgb("#DDE3EF");

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

    public LanguageLevelResponse? SelectedLevel
    {
        get => _selectedLevel;
        set => SetField(ref _selectedLevel, value);
    }

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
            ErrorMessage = $"Не удалось загрузить данные регистрации: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NextStep()
    {
        ErrorMessage = "";

        if (Step == 1 && !ValidateStep1())
            return;

        if (Step == 2 && !ValidateStep2())
            return;

        if (Step < 3)
            Step++;
    }

    private async void PreviousStep()
    {
        ErrorMessage = "";

        if (Step > 1)
            Step--;
        else await BackAsync();
    }

    private bool ValidateStep1()
    {
        var email = Email.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "Введите адрес электронной почты.";
            return false;
        }

        if (!IsValidEmail(email))
        {
            ErrorMessage = "Введите корректный адрес электронной почты.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите пароль.";
            return false;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Пароль должен содержать минимум 6 символов.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Повторите пароль.";
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return false;
        }

        return true;
    }

    private bool ValidateStep2()
    {
        if (SelectedNativeLanguage == null || SelectedTargetLanguage == null)
        {
            ErrorMessage = "Выберите родной и изучаемый язык.";
            return false;
        }

        if (SelectedNativeLanguage.Id == SelectedTargetLanguage.Id)
        {
            ErrorMessage = "Родной и изучаемый язык должны отличаться.";
            return false;
        }

        if (SelectedLevel == null)
        {
            ErrorMessage = "Выберите уровень.";
            return false;
        }

        return true;
    }

    private async Task RegisterAsync()
    {
        ErrorMessage = "";

        if (!ValidateStep1())
        {
            Step = 1;
            return;
        }

        if (!ValidateStep2())
        {
            Step = 2;
            return;
        }

        try
        {
            IsBusy = true;

            var response = await _apiClient.RegisterAsync(new RegisterRequest
            {
                Email = Email.Trim(),
                Password = Password,
                NativeLanguageId = SelectedNativeLanguage!.Id,
                TargetLanguageId = SelectedTargetLanguage!.Id,
                LanguageLevelId = SelectedLevel!.Id
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

    private async Task BackAsync()
    {
        var welcomePage = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<WelcomePage>();

        Application.Current.Windows[0].Page = new NavigationPage(welcomePage);

        await Task.CompletedTask;

    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        OnPropertyChanged(nameof(CanContinueStep));
        return true;
    }

    private void StepPropertyChanged()
    {
        OnPropertyChanged(nameof(Step));
        OnPropertyChanged(nameof(IsStep1));
        OnPropertyChanged(nameof(IsStep2));
        OnPropertyChanged(nameof(IsStep3));
        OnPropertyChanged(nameof(StepTitle));
        OnPropertyChanged(nameof(StepSubtitle));
        OnPropertyChanged(nameof(StepCounterText));
        OnPropertyChanged(nameof(Step1Color));
        OnPropertyChanged(nameof(Step2Color));
        OnPropertyChanged(nameof(Step3Color));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}