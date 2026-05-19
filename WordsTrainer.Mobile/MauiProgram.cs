using Microsoft.Extensions.Logging;
using WordsTrainer.Mobile.Services;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.ViewModels;

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

        builder.Services.AddSingleton<TokenStorage>();

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5261");
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
#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
