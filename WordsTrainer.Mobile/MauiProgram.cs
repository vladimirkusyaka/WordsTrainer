using Microsoft.Extensions.Logging;
using WordsTrainer.Mobile.Configuration;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;
using WordsTrainer.Mobile.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers;

namespace WordsTrainer.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.UseMauiCommunityToolkit();

        builder.Services.AddSingleton<TokenStorage>();

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiBaseUrl);
        });

        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();

        builder.Services.AddTransient<TrainingPage>();
        builder.Services.AddTransient<TrainingViewModel>();

        builder.Services.AddTransient<ExplanationPage>();
        builder.Services.AddTransient<ExplanationViewModel>();

        builder.Services.AddSingleton<StartupService>();

        builder.Services.AddSingleton<UiTextService>();

        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<WelcomeViewModel>();

        PickerHandler.Mapper.AppendToMapping("NoArrow", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(
                    Android.Graphics.Color.Transparent);
#endif
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
