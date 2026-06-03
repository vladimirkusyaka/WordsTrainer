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
    private readonly UiTextService _texts;
    private readonly TrainingReminderService _trainingReminderService;

    private int _step = 1;
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _errorMessage = "";
    private bool _isBusy;

    private LanguageResponse? _selectedNativeLanguage;
    private LanguageResponse? _selectedTargetLanguage;
    private LanguageLevelResponse? _selectedLevel;

    public RegisterViewModel(
        ApiClient apiClient,
        TokenStorage tokenStorage,
        UiTextService texts,
        TrainingReminderService trainingReminderService)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _texts = texts;
        _trainingReminderService = trainingReminderService;

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
        1 => _texts.T("register.title.1"),
        2 => _texts.T("register.title.2"),
        3 => _texts.T("register.title.3"),
        _ => _texts.T("create.account")
    };

    public string StepSubtitle => Step switch
    {
        1 => _texts.T("register.subtitle.1"),
        2 => _texts.T("register.subtitle.2"),
        3 => _texts.T("register.subtitle.3"),
        _ => ""
    };

    public string StepCounterText => _texts.Format("register.step", Step);

    public string BackText => _texts.T("back");
    public string EmailText => _texts.T("email");
    public string EmailPlaceholderText => _texts.T("email.placeholder");
    public string PasswordText => _texts.T("password");
    public string PasswordPlaceholderText => _texts.T("password.placeholder");
    public string ConfirmPasswordText => _texts.T("confirm.password");
    public string ConfirmPasswordPlaceholderText => _texts.T("confirm.password.placeholder");
    public string NativeLanguageText => _texts.T("native.language");
    public string TargetLanguageText => _texts.T("target.language");
    public string CurrentLevelText => _texts.T("current.level");
    public string AccountSummaryText => _texts.T("account.summary");
    public string LearningText => _texts.T("learning");
    public string ContinueText => _texts.T("continue");
    public string CreateAccountText => _texts.T("create.account");
    public string AlreadyHaveAccountText => _texts.T("already.account");
    public string SignInText => _texts.T("sign.in");

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
        set
        {
            if (SetField(ref _selectedNativeLanguage, value))
            {
                _texts.SetLanguage(value?.Code);
                NotifyLocalizedProperties();
            }
        }
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
        catch
        {
            ErrorMessage = _texts.T("register.load.failed");
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
            ErrorMessage = _texts.T("register.email.required");
            return false;
        }

        if (!IsValidEmail(email))
        {
            ErrorMessage = _texts.T("register.email.invalid");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = _texts.T("register.password.required");
            return false;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = _texts.T("register.password.short");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = _texts.T("register.confirm.required");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = _texts.T("register.passwords.mismatch");
            return false;
        }

        return true;
    }

    private bool ValidateStep2()
    {
        if (SelectedNativeLanguage == null || SelectedTargetLanguage == null)
        {
            ErrorMessage = _texts.T("register.languages.required");
            return false;
        }

        if (SelectedNativeLanguage.Id == SelectedTargetLanguage.Id)
        {
            ErrorMessage = _texts.T("register.languages.same");
            return false;
        }

        if (SelectedLevel == null)
        {
            ErrorMessage = _texts.T("register.level.required");
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
                ErrorMessage = _texts.T("register.failed");
                return;
            }

            await _tokenStorage.SaveAccessTokenAsync(response.AccessToken);
            await _trainingReminderService.ScheduleDailyReminderAsync(skipToday: true);

            var trainingPage = Application.Current!.Handler!.MauiContext!.Services
    .GetRequiredService<TrainingPage>();

            await trainingPage.InitializeAsync();

            Application.Current.Windows[0].Page = new NavigationPage(trainingPage);
        }
        catch
        {
            ErrorMessage = _texts.T("register.failed");
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

    private void NotifyLocalizedProperties()
    {
        OnPropertyChanged(nameof(BackText));
        OnPropertyChanged(nameof(EmailText));
        OnPropertyChanged(nameof(EmailPlaceholderText));
        OnPropertyChanged(nameof(PasswordText));
        OnPropertyChanged(nameof(PasswordPlaceholderText));
        OnPropertyChanged(nameof(ConfirmPasswordText));
        OnPropertyChanged(nameof(ConfirmPasswordPlaceholderText));
        OnPropertyChanged(nameof(NativeLanguageText));
        OnPropertyChanged(nameof(TargetLanguageText));
        OnPropertyChanged(nameof(CurrentLevelText));
        OnPropertyChanged(nameof(AccountSummaryText));
        OnPropertyChanged(nameof(LearningText));
        OnPropertyChanged(nameof(ContinueText));
        OnPropertyChanged(nameof(CreateAccountText));
        OnPropertyChanged(nameof(AlreadyHaveAccountText));
        OnPropertyChanged(nameof(SignInText));
        StepPropertyChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
