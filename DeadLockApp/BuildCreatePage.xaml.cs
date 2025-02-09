namespace DeadLockApp;
using DeadLockApp.ViewModels;
public partial class BuildCreatePage : ContentPage
{
    private BuildCreateViewModel _viewModel;

    public BuildCreatePage()
    {
        InitializeComponent();
        _viewModel = new BuildCreateViewModel();
        BindingContext = _viewModel;
    }
}