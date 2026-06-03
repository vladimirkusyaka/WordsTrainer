#if ANDROID
using Android.App;
using Android.Content;

namespace WordsTrainer.Mobile.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter([Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced])]
public sealed class TrainingReminderBootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || !TrainingReminderScheduler.IsEnabled(context))
            return;

        TrainingReminderScheduler.ScheduleDaily(
            context,
            skipToday: TrainingReminderScheduler.IsDailyGoalCompletedToday(context));
    }
}
#endif
