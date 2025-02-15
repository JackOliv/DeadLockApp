using DeadLockApp.ViewModels;

namespace DeadLockApp;

[QueryProperty(nameof(BuildId), "buildId")]
[QueryProperty(nameof(BuildName), "buildName")]
[QueryProperty(nameof(CharacterId), "characterId")]
public partial class BuildDetailsPage : ContentPage
{
    public int BuildId { get; set; }
    public string BuildName { get; set; }
    public int CharacterId { get; set; }

    private readonly BuildDetailsViewModel _viewModel;

    public BuildDetailsPage()
    {
        InitializeComponent();
        _viewModel = new BuildDetailsViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBuildDetailsAsync(CharacterId, BuildId);
    }
}
