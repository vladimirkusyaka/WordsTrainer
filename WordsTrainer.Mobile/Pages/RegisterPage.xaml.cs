using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;
    private bool _initialized;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_initialized)
            return;

        _initialized = true;
        await _viewModel.InitializeAsync();
    }
}