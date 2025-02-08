using System.Collections.ObjectModel;
using System.Windows.Input;
using DeadLockApp.Models;

namespace DeadLockApp.ViewModels
{
    public class BuildCreateViewModel : BaseViewModel
    {
        public ObservableCollection<Item> StartItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> MiddleItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> EndItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> SituationalItems { get; set; } = new ObservableCollection<Item>();

        private string _buildName;
        public string BuildName
        {
            get => _buildName;
            set
            {
                _buildName = value;
                OnPropertyChanged(nameof(BuildName));
            }
        }

        public ICommand AddStartItemCommand { get; }
        public ICommand AddMiddleItemCommand { get; }
        public ICommand AddEndItemCommand { get; }
        public ICommand AddSituationalItemCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand SaveBuildCommand { get; }

        public BuildCreateViewModel()
        {
            AddStartItemCommand = new Command(() => OpenItemSelection(1));
            AddMiddleItemCommand = new Command(() => OpenItemSelection(2));
            AddEndItemCommand = new Command(() => OpenItemSelection(3));
            AddSituationalItemCommand = new Command(() => OpenItemSelection(4));
            RemoveCommand = new Command<Item>(RemoveItem);
            SaveBuildCommand = new Command(SaveBuild);
        }

        private async void OpenItemSelection(int partId)
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ItemSelectionPage(partId, this));
        }

        public void AddItemToBuild(Item item, int partId)
        {
            switch (partId)
            {
                case 1: StartItems.Add(item); break;
                case 2: MiddleItems.Add(item); break;
                case 3: EndItems.Add(item); break;
                case 4: SituationalItems.Add(item); break;
            }
        }

        private void RemoveItem(Item item)
        {
            StartItems.Remove(item);
            MiddleItems.Remove(item);
            EndItems.Remove(item);
            SituationalItems.Remove(item);
        }

        private async void SaveBuild()
        {
            // Логика сохранения билда через API
            await Application.Current.MainPage.DisplayAlert("Успех", "Билд сохранен!", "Ок");
        }
    }
}
