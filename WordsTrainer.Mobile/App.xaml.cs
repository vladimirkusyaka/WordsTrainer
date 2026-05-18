using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile;

public partial class App : Application
{
    private readonly StartupService _startupService;

    public App(StartupService startupService)
    {
        InitializeComponent();
        _startupService = startupService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var page = new ContentPage
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            }
        };

        var window = new Window(page);

        _ = InitializeAsync(window);

        return window;
    }

    private async Task InitializeAsync(Window window)
    {
        var rootPage = await _startupService.GetStartupPageAsync();
        window.Page = rootPage;
    }
}