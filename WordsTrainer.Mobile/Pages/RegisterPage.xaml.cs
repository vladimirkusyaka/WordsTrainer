using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;
    private bool _initialized;
    private bool _isPasswordVisible;
    private bool _isConfirmPasswordVisible;

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

    private void TogglePasswordClicked(object? sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        PasswordEntry.IsPassword = !_isPasswordVisible;

        if (sender is ImageButton button)
        {
            button.Source = _isPasswordVisible
                ? "eye_off.svg"
                : "eye.svg";
        }
    }

    private void ToggleConfirmPasswordClicked(object? sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;

        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;

        if (sender is ImageButton button)
        {
            button.Source = _isConfirmPasswordVisible
                ? "eye_off.svg"
                : "eye.svg";
        }
    }
}