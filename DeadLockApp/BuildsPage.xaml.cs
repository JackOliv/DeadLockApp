using DeadLockApp.Models;
using DeadLockApp.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace DeadLockApp
{
    [QueryProperty(nameof(CharacterId), "characterId")]
    [QueryProperty(nameof(CharacterName), "name")]
    public partial class BuildsPage : ContentPage
    {
        public int CharacterId { get; set; }
        public string CharacterName { get; set; }
        private BuildsViewModel _viewModel;

        public BuildsPage()
        {
            InitializeComponent();
            _viewModel = new BuildsViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Builds.Clear();
            PageTitle.Text = $"Сборки {CharacterName}";
            // Принудительная загрузка данных каждый раз при появлении
            Task.Run(async () => await _viewModel.LoadBuildsAsync(CharacterId));
        }
        private async void OnBuildSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Build selectedBuild)
            {
               
                    string route = $"{nameof(BuildDetailsPage)}?buildId={selectedBuild.Id}&buildName={selectedBuild.Name}&characterId={CharacterId}";
                    await Shell.Current.GoToAsync(route);
               
            }
        }
        private async void OnCreateBuild(object sender, EventArgs e)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (token != null)
            {
                Debug.WriteLine(token);
                string route = $"{nameof(BuildCreatePage)}?characterId={CharacterId}";
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
        }


    }


}
