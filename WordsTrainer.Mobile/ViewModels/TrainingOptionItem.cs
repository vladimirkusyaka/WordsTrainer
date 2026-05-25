using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WordsTrainer.Mobile.ViewModels;

public enum TrainingAnswerState
{
    Normal,
    Correct,
    Incorrect
}

public class TrainingOptionItem : INotifyPropertyChanged
{
    public Guid WordId { get; init; }

    public string Text { get; init; } = string.Empty;

    private TrainingAnswerState _state;

    public TrainingAnswerState State
    {
        get => _state;
        set
        {
            if (_state == value)
                return;

            _state = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(BorderColor));
        }
    }

    public string DisplayText => State switch
    {
        TrainingAnswerState.Correct => $"✓  {Text}",
        TrainingAnswerState.Incorrect => $"×  {Text}",
        _ => Text
    };

    public Color BackgroundColor => State switch
    {
        TrainingAnswerState.Correct => Color.FromArgb("#DCFCE7"),
        TrainingAnswerState.Incorrect => Color.FromArgb("#FEE2E2"),
        _ => Color.FromArgb("#FFFFFF")
    };

    public Color TextColor => State switch
    {
        TrainingAnswerState.Correct => Color.FromArgb("#15803D"),
        TrainingAnswerState.Incorrect => Color.FromArgb("#B91C1C"),
        _ => Color.FromArgb("#111827")
    };

    public Color BorderColor => State switch
    {
        TrainingAnswerState.Correct => Color.FromArgb("#22C55E"),
        TrainingAnswerState.Incorrect => Color.FromArgb("#EF4444"),
        _ => Color.FromArgb("#E2E8F0")
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
