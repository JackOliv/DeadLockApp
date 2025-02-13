using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
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
            string route = $"{nameof(ItemSelectionPage)}?partId={partId}";
            await Shell.Current.GoToAsync(route);
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
            try
            {
                if (string.IsNullOrEmpty(BuildName))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название билда.", "Ок");
                    return;
                }

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Не найден токен авторизации.", "Ок");
                    return;
                }

                var buildRequest = new
                {
                    name = BuildName,
                    character_id = 1, // Пока что фиксированный ID персонажа, можно изменить
                    items = GetItemsList()
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(
                    JsonSerializer.Serialize(buildRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("http://192.168.2.20/api/builds/create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Успех", "Билд сохранен!", "Ок");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка сохранения билда: {responseContent}", "Ок");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка запроса: {ex.Message}", "Ок");
            }
        }
        private List<object> GetItemsList()
        {
            var items = new List<object>();

            void AddItems(IEnumerable<Item> collection, int partId)
            {
                foreach (var item in collection)
                {
                    items.Add(new { item_id = item.Id, part = partId });
                }
            }

            AddItems(StartItems, 1);
            AddItems(MiddleItems, 2);
            AddItems(EndItems, 3);
            AddItems(SituationalItems, 4);

            return items;
        }

    }
}
