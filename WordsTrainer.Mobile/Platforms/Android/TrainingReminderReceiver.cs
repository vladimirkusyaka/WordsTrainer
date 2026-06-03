#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;

namespace WordsTrainer.Mobile.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class TrainingReminderReceiver : BroadcastReceiver
{
    private const int NotificationId = 1701;
    private const string ChannelId = "daily_training";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null)
            return;

        if (TrainingReminderScheduler.IsDailyGoalCompletedToday(context))
            return;

        EnsureChannel(context);

        var (title, message) = TrainingReminderScheduler.GetReminderText(context);
        var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName!);

        launchIntent?.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

        var contentIntent = PendingIntent.GetActivity(
            context,
            0,
            launchIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var builder = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? new Notification.Builder(context, ChannelId)
            : new Notification.Builder(context);

        var notification = builder
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.ic_notification)
            .SetContentIntent(contentIntent)
            .SetAutoCancel(true)
            .Build();

        var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        notificationManager?.Notify(NotificationId, notification);
    }

    private static void EnsureChannel(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);

        if (notificationManager?.GetNotificationChannel(ChannelId) != null)
            return;

        var channel = new NotificationChannel(
            ChannelId,
            "Daily training",
            NotificationImportance.Default)
        {
            Description = "Daily reminder to train words"
        };

        notificationManager?.CreateNotificationChannel(channel);
    }
}
#endif
