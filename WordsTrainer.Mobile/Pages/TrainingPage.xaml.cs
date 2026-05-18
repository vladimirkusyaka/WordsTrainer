using WordsTrainer.Mobile.ViewModels;

namespace WordsTrainer.Mobile.Pages;

public partial class TrainingPage : ContentPage
{
    private readonly TrainingViewModel _viewModel;
    private bool _initialized;

    public TrainingPage(TrainingViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        await _viewModel.InitializeAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await InitializeAsync();
    }
}