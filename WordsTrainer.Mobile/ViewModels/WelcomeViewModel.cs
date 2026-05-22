using System.Windows.Input;
using WordsTrainer.Mobile.Pages;

namespace WordsTrainer.Mobile.ViewModels;

public class WelcomeViewModel
{
    public ICommand GetStartedCommand { get; }
    public ICommand LoginCommand { get; }

    public WelcomeViewModel()
    {
        GetStartedCommand = new Command(async () => await GoToRegisterAsync());
        LoginCommand = new Command(async () => await GoToLoginAsync());
    }

    private async Task GoToRegisterAsync()
    {
        var registerPage = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<RegisterPage>();

        Application.Current.Windows[0].Page = new NavigationPage(registerPage);

        await Task.CompletedTask;
    }

    private async Task GoToLoginAsync()
    {
        var loginPage = Application.Current!.Handler!.MauiContext!.Services
            .GetRequiredService<LoginPage>();

        Application.Current.Windows[0].Page = new NavigationPage(loginPage);

        await Task.CompletedTask;
    }
}
