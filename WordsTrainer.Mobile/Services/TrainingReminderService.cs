namespace WordsTrainer.Mobile.Services;

public class TrainingReminderService
{
    private readonly UiTextService _texts;

    public TrainingReminderService(UiTextService texts)
    {
        _texts = texts;
    }

    public async Task ScheduleDailyReminderAsync(bool skipToday = false)
    {
#if ANDROID
        await Platforms.Android.TrainingReminderScheduler.ScheduleDailyAsync(
            _texts.T("notification.training.title"),
            _texts.T("notification.training.message"),
            skipToday);
#else
        await Task.CompletedTask;
#endif
    }

    public async Task MarkDailyGoalCompletedAsync()
    {
#if ANDROID
        await Platforms.Android.TrainingReminderScheduler.MarkDailyGoalCompletedAsync(
            _texts.T("notification.training.title"),
            _texts.T("notification.training.message"));
#else
        await Task.CompletedTask;
#endif
    }
}
