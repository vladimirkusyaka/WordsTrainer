#if ANDROID
using Android.App;
using Android.Content;
using Microsoft.Maui.ApplicationModel;

namespace WordsTrainer.Mobile.Platforms.Android;

internal static class TrainingReminderScheduler
{
    public const string ActionShowTrainingReminder = "WordsTrainer.Mobile.SHOW_TRAINING_REMINDER";

    private const int RequestCode = 1700;
    private const string PreferencesName = "words_trainer_reminder";
    private const string EnabledKey = "enabled";
    private const string TitleKey = "title";
    private const string MessageKey = "message";
    private const string CompletedDateKey = "completed_date";
    private const string DateFormat = "yyyy-MM-dd";

    public static async Task ScheduleDailyAsync(
        string title,
        string message,
        bool skipToday = false)
    {
        if (!await EnsureNotificationPermissionAsync())
            return;

        var context = Platform.AppContext ?? global::Android.App.Application.Context;
        SaveReminderText(context, title, message);
        ScheduleDaily(context, skipToday || IsDailyGoalCompletedToday(context));
    }

    public static async Task MarkDailyGoalCompletedAsync(string title, string message)
    {
        var context = Platform.AppContext ?? global::Android.App.Application.Context;
        SaveReminderText(context, title, message);

        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
        var editor = preferences?.Edit();

        editor?.PutString(CompletedDateKey, DateTime.Today.ToString(DateFormat));
        editor?.Apply();

        if (!await EnsureNotificationPermissionAsync())
            return;

        ScheduleDaily(context, skipToday: true);
    }

    public static void ScheduleDaily(Context context, bool skipToday = false)
    {
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager == null)
            return;

        var intent = new Intent(context, typeof(TrainingReminderReceiver));
        intent.SetAction(ActionShowTrainingReminder);

        var pendingIntent = PendingIntent.GetBroadcast(
            context,
            RequestCode,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (pendingIntent == null)
            return;

        alarmManager.SetInexactRepeating(
            AlarmType.RtcWakeup,
            GetNextReminderTimeMilliseconds(skipToday),
            AlarmManager.IntervalDay,
            pendingIntent);
    }

    public static (string Title, string Message) GetReminderText(Context context)
    {
        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);

        return (
            preferences?.GetString(TitleKey, "Time to train") ?? "Time to train",
            preferences?.GetString(MessageKey, "Practice a few words today.") ?? "Practice a few words today.");
    }

    public static bool IsEnabled(Context context)
    {
        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
        return preferences?.GetBoolean(EnabledKey, false) == true;
    }

    public static bool IsDailyGoalCompletedToday(Context context)
    {
        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
        var completedDate = preferences?.GetString(CompletedDateKey, null);

        return string.Equals(
            completedDate,
            DateTime.Today.ToString(DateFormat),
            StringComparison.Ordinal);
    }

    private static void SaveReminderText(Context context, string title, string message)
    {
        var preferences = context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
        var editor = preferences?.Edit();

        editor?.PutBoolean(EnabledKey, true);
        editor?.PutString(TitleKey, title);
        editor?.PutString(MessageKey, message);
        editor?.Apply();
    }

    private static async Task<bool> EnsureNotificationPermissionAsync()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return true;

        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.PostNotifications>();

        return status == PermissionStatus.Granted;
    }

    private static long GetNextReminderTimeMilliseconds(bool skipToday)
    {
        var next = DateTime.Today.AddHours(17);

        if (skipToday || next <= DateTime.Now)
            next = next.AddDays(1);

        return new DateTimeOffset(next).ToUnixTimeMilliseconds();
    }
}
#endif
