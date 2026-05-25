using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WordsTrainer.Contracts.Training;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels;

public class ExplanationViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;

    private Guid _attemptId;
    private Guid _correctWordId;
    private string _targetWord = "";
    private string _nativeTranslation = "";
    private string _explanation = "";
    private string _questionLevel = "";
    private string _message = "";
    private bool _isBusy;
    private bool _wasSubmitted;

    public ExplanationViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        PlayAudioCommand = new Command(async () => await PlayAudioAsync());
    }

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

            var response = await _apiClient.GetExplanationAsync(attemptId);

            if (response == null)
            {
                Message = "Explanation not found.";
                return;
            }

            _attemptId = response.AttemptId;
            _correctWordId = response.CorrectWordId;

            TargetWord = response.TargetWord;
            NativeTranslation = response.NativeTranslation;
            Explanation = response.Explanation;
            QuestionLevel = response.TargetLevelCode;
            AudioUrl = response.AudioUrl;
        }
        catch (Exception ex)
        {
            Message = $"Unable to load explanation: {ex.Message}";
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
            Message = "Unable to continue: refresh the API and reload this word.";
            return false;
        }

        try
        {
            IsBusy = true;
            Message = "";

            var response = await _apiClient.SubmitAnswerAsync(new SubmitTrainingAnswerRequest
            {
                AttemptId = _attemptId,
                SelectedWordId = _correctWordId,
                TranslationViewed = true,
                DurationMs = 0
            });

            if (response == null)
            {
                Message = "Unable to save this word for review.";
                return false;
            }

            _wasSubmitted = true;
            return true;
        }
        catch (Exception ex)
        {
            Message = $"Unable to continue: {ex.Message}";
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
            ? "Audio is not available yet."
            : "Audio playback will be available soon.";

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