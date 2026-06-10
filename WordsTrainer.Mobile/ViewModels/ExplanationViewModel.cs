using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WordsTrainer.Contracts.Training;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels;

public class ExplanationViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly UiTextService _texts;

    private Guid _attemptId;
    private Guid _correctWordId;
    private string _targetWord = "";
    private string _nativeTranslation = "";
    private string _explanation = "";
    private string _questionLevel = "";
    private string _message = "";
    private bool _isBusy;
    private bool _wasSubmitted;

    public ExplanationViewModel(ApiClient apiClient, UiTextService texts)
    {
        _apiClient = apiClient;
        _texts = texts;
        PlayAudioCommand = new Command(async () => await PlayAudioAsync());
    }

    public string BackText => _texts.T("back");
    public string ExplanationTitleText => _texts.T("explanation.title");
    public string GotItContinueText => _texts.T("got.it.continue");
    public string MarkedForReviewText => _texts.T("marked.for.review");

    public string TargetWord
    {
        get => _targetWord;
        set => SetField(ref _targetWord, value);
    }

    public string NativeTranslation
    {
        get => _nativeTranslation;
        set => SetField(ref _nativeTranslation, value);
    }

    public string Explanation
    {
        get => _explanation;
        set => SetField(ref _explanation, value);
    }

    public string QuestionLevel
    {
        get => _questionLevel;
        set
        {
            if (SetField(ref _questionLevel, value))
            {
                OnPropertyChanged(nameof(HasQuestionLevel));
            }
        }
    }

    public bool HasQuestionLevel => !string.IsNullOrWhiteSpace(QuestionLevel);

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

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string? AudioUrl { get; private set; }

    public ICommand PlayAudioCommand { get; }

    public async Task LoadAsync(Guid attemptId)
    {
        try
        {
            IsBusy = true;
            Message = "";
            _wasSubmitted = false;
            QuestionLevel = "";

            var result = await _apiClient.GetExplanationAsync(attemptId);

            if (!result.IsSuccess || result.Value == null)
            {
                Message = result.ToDisplayMessage(_texts, "explanation.not.found");
                return;
            }

            var response = result.Value;

            _attemptId = response.AttemptId;
            _correctWordId = response.CorrectWordId;

            TargetWord = response.TargetWord;
            NativeTranslation = response.NativeTranslation;
            Explanation = response.Explanation;
            QuestionLevel = response.TargetLevelCode;
            AudioUrl = response.AudioUrl;
        }
        catch
        {
            Message = _texts.T("explanation.load.failed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> ContinueAsync()
    {
        if (IsBusy)
            return false;

        if (_wasSubmitted)
            return true;

        if (_attemptId == Guid.Empty || _correctWordId == Guid.Empty)
        {
            Message = _texts.T("explanation.continue.reload");
            return false;
        }

        try
        {
            IsBusy = true;
            Message = "";

            var result = await _apiClient.SubmitAnswerAsync(new SubmitTrainingAnswerRequest
            {
                AttemptId = _attemptId,
                SelectedWordId = _correctWordId,
                TranslationViewed = true,
                DurationMs = 0
            });

            if (!result.IsSuccess || result.Value == null)
            {
                Message = result.ToDisplayMessage(_texts, "explanation.save.failed");
                return false;
            }

            _wasSubmitted = true;
            return true;
        }
        catch
        {
            Message = _texts.T("explanation.continue.failed");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PlayAudioAsync()
    {
        Message = string.IsNullOrWhiteSpace(AudioUrl)
            ? _texts.T("audio.not.available")
            : _texts.T("audio.coming.soon");

        await Task.CompletedTask;
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
        OnPropertyChanged(name);
        return true;
    }
}
