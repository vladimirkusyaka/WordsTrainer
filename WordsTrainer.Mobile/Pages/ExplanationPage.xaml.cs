using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages;

public partial class ExplanationPage : ContentPage
{
    private readonly ExplanationViewModel _viewModel;

    public event EventHandler? ContinueCompleted;

    public ExplanationPage(ExplanationViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public async Task LoadAsync(Guid attemptId)
    {
        await _viewModel.LoadAsync(attemptId);
    }

    private async void BackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void ContinueClicked(object? sender, EventArgs e)
    {
        var saved = await _viewModel.ContinueAsync();

        if (!saved)
            return;

        await Navigation.PopAsync();

        ContinueCompleted?.Invoke(this, EventArgs.Empty);
    }
}