using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using DeadLockApp.Models;
using DeadLockApp.ViewModels;

namespace DeadLockApp
{
    [QueryProperty(nameof(BuildJson), "build")]
    public partial class BuildEditPage : ContentPage
    {
        private Build _currentBuild;
        private BuildEditViewModel _viewModel;

        public string BuildJson
        {
            set
            {
                _currentBuild = JsonSerializer.Deserialize<Build>(Uri.UnescapeDataString(value));
            }
        }

        public BuildEditPage()
        {
            InitializeComponent();
            _viewModel = new BuildEditViewModel();
            BindingContext = _viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_currentBuild != null)
            {
                await _viewModel.LoadBuildData(_currentBuild);
            }
        }

    }


}
