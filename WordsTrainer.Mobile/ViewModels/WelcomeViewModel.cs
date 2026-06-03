using System.Windows.Input;
using WordsTrainer.Mobile.Pages;
using WordsTrainer.Mobile.Services;

namespace WordsTrainer.Mobile.ViewModels;

public class WelcomeViewModel
{
    private readonly UiTextService _texts;

    public ICommand GetStartedCommand { get; }
    public ICommand LoginCommand { get; }

    public WelcomeViewModel(UiTextService texts)
    {
        _texts = texts;
        GetStartedCommand = new Command(async () => await GoToRegisterAsync());
        LoginCommand = new Command(async () => await GoToLoginAsync());
    }

    public string TitleText => _texts.T("welcome.hero.title");
    public string SubtitleText => _texts.T("welcome.hero.subtitle");
    public string DescriptionText => _texts.T("welcome.hero.description");
    public string LanguagesText => _texts.T("welcome.languages");
    public string PracticeText => _texts.T("welcome.practice");
    public string GetStartedText => _texts.T("get.started");
    public string AlreadyHaveAccountText => _texts.T("already.have.account");

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
