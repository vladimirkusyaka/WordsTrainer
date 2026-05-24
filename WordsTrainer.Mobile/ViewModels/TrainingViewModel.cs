using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WordsTrainer.Contracts.Training;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Maui.Controls.Application;

namespace WordsTrainer.Mobile.ViewModels;

public class TrainingViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly UiTextService _texts;

    private TrainingQuestionResponse? _currentQuestion;

    private string _questionText = "";
    private string _message = "";
    private bool _isBusy;
    private bool _hasQuestion;
    private bool _hasNoQuestion = true;

    private int _answeredToday;
    private int _correctToday;
    private int _newCorrectToday;
    private int _learnedTotal;

    public string AppTitle => _texts.T("app.title");
    public string AppSubtitle => _texts.T("app.subtitle");
    public string LogoutText => _texts.T("logout");
    public string TodayText => _texts.T("today");
    public string CorrectText => _texts.T("correct");
    public string NewText => _texts.T("new");
    public string LearnedText => _texts.T("learned");
    public string TranslateWordText => _texts.T("translate.word");
    public string DontKnowText => _texts.T("dont.know");
    public string TrainingCompleteText => _texts.T("training.complete");

    public TrainingViewModel(ApiClient apiClient, TokenStorage tokenStorage,
    UiTextService texts)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _texts = texts;

        AnswerCommand = new Command<TrainingOptionDto>(
            async option => await AnswerAsync(option),
            _ => !IsBusy && HasQuestion);

        ShowExplanationCommand = new Command(
            async () => await ShowExplanationAsync(),
            () => !IsBusy && HasQuestion);

        LogoutCommand = new Command(
            async () => await LogoutAsync(),
            () => !IsBusy);
    }

    public string QuestionText
    {
        get => _questionText;
        set => SetField(ref _questionText, value);
    }

    public string Message
    {
        get => _message;
        set
        {
            if (SetField(ref _message, value))
            {
                OnPropertyChanged(nameof(HasMessage));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
            {
                RefreshCommands();
            }
        }
    }

    public bool HasQuestion
    {
        get => _hasQuestion;
        set
        {
            if (SetField(ref _hasQuestion, value))
            {
                HasNoQuestion = !value;
                RefreshCommands();
            }
        }
    }

    private string _levelCode = "";

    public string LevelCode
    {
        get => _levelCode;
        set => SetField(ref _levelCode, value);
    }

    public bool HasNoQuestion
    {
        get => _hasNoQuestion;
        set => SetField(ref _hasNoQuestion, value);
    }

    public List<TrainingOptionDto> Options { get; private set; } = [];

    private string _progressText = string.Empty;
    public string ProgressText
    {
        get => _progressText;
        set => SetField(ref _progressText, value);
    }

    private double _trainingProgress;
    public double TrainingProgress
    {
        get => _trainingProgress;
        set => SetField(ref _trainingProgress, value);
    }

    private string _questionLevel = "A1";
    public string QuestionLevel
    {
        get => _questionLevel;
        set => SetField(ref _questionLevel, value);
    }


    public int AnsweredToday
    {
        get => _answeredToday;
        set => SetField(ref _answeredToday, value);
    }

    public int CorrectToday
    {
        get => _correctToday;
        set => SetField(ref _correctToday, value);
    }

    public int NewCorrectToday
    {
        get => _newCorrectToday;
        set => SetField(ref _newCorrectToday, value);
    }

    public int LearnedTotal
    {
        get => _learnedTotal;
        set => SetField(ref _learnedTotal, value);
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public ICommand AnswerCommand { get; }
    public ICommand ShowExplanationCommand { get; }
    public ICommand LogoutCommand { get; }

    public async Task InitializeAsync()
    {
        await LoadCurrentUserAsync();
        await LoadStatsAsync();
        await LoadNextAsync();
    }

    private int currentQuestionIndex { get; set; } = 0;
    private int totalQuestions { get; set; } = 10;
    private async Task LoadNextAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var next = await _apiClient.GetNextAsync();

            if (next == null)
            {
                ClearQuestion("Unable to load next question.");
                return;
            }

            if (next.Status != TrainingNextStatus.Available || next.Question == null)
            {
                ClearQuestion(GetFriendlyMessage(next.Status, next.Message));
                return;
            }

            _currentQuestion = next.Question;
            QuestionText = next.Question.Question;
            Options = next.Question.Options;
            OnPropertyChanged(nameof(Options));

            ProgressText = $"{currentQuestionIndex + 1} of {totalQuestions} words";
            TrainingProgress = totalQuestions == 0
                ? 0
                : (double)(currentQuestionIndex + 1) / totalQuestions;

            QuestionLevel = "A1";  // TEMPORARY!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            HasQuestion = true;
            Message = "";
        }
        catch (Exception ex)
        {
            ClearQuestion($"⚠ Unable to load next question.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AnswerAsync(TrainingOptionDto? option)
    {
        if (option == null || _currentQuestion == null || IsBusy)
            return;

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var response = await _apiClient.SubmitAnswerAsync(new SubmitTrainingAnswerRequest
            {
                AttemptId = _currentQuestion.AttemptId,
                SelectedWordId = option.WordId,
                TranslationViewed = false,
                DurationMs = 3000
            });

            if (response == null)
            {
                Message = "Ответ не принят.";
                return;
            }

            Message = response.IsCorrect
                    ? _texts.T("correct.answer")
                    : string.Format(_texts.T("wrong.answer"), response.CorrectAnswer);

            await LoadStatsAsync();
        }
        catch (Exception ex)
        {
            Message = $"Ошибка ответа: {ex.Message}";
            return;
        }
        finally
        {
            IsBusy = false;
        }

        await Task.Delay(700);

        await LoadNextAsync();
    }

    private async Task ShowExplanationAsync()
    {
        if (_currentQuestion == null || IsBusy)
            return;

        var page = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<ExplanationPage>();

        await page.LoadAsync(_currentQuestion.AttemptId);

        await Application.Current.Windows[0].Page!.Navigation.PushAsync(page);
    }

    private async Task LogoutAsync()
    {
        _tokenStorage.Clear();

        var loginPage = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<LoginPage>();

        Application.Current.Windows[0].Page = new NavigationPage(loginPage);
        OnPropertyChanged("Logout");

        await Task.CompletedTask;
    }

    private async Task LoadStatsAsync()
    {
        var stats = await _apiClient.GetStatsAsync();

        if (stats == null)
            return;

        AnsweredToday = stats.AnsweredToday;
        CorrectToday = stats.CorrectToday;
        NewCorrectToday = stats.NewCorrectToday;
        LearnedTotal = stats.LearnedTotal;
    }

    private void ClearQuestion(string message)
    {
        _currentQuestion = null;

        QuestionText = "";
        Options = [];
        OnPropertyChanged(nameof(Options));

        HasQuestion = false;
        Message = message;
    }

    private void RefreshCommands()
    {
        ((Command<TrainingOptionDto>)AnswerCommand).ChangeCanExecute();
        ((Command)ShowExplanationCommand).ChangeCanExecute();
        ((Command)LogoutCommand).ChangeCanExecute();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private async Task LoadCurrentUserAsync()
    {
        var me = await _apiClient.GetMeAsync();

        if (me == null)
            return;

        _texts.SetLanguage(me.NativeLanguageCode);

        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(AppSubtitle));
        OnPropertyChanged(nameof(LogoutText));
        OnPropertyChanged(nameof(TodayText));
        OnPropertyChanged(nameof(CorrectText));
        OnPropertyChanged(nameof(NewText));
        OnPropertyChanged(nameof(LearnedText));
        OnPropertyChanged(nameof(TranslateWordText));
        OnPropertyChanged(nameof(DontKnowText));
        OnPropertyChanged(nameof(TrainingCompleteText));
    }

    private string GetFriendlyMessage(TrainingNextStatus status, string? backendMessage)
    {
        return status switch
        {
            TrainingNextStatus.SessionCompleted => _texts.T("session.complete"),
            TrainingNextStatus.NoWordsAvailable => _texts.T("no.words"),
            _ => backendMessage ?? _texts.T("no.words")
        };
    }
}