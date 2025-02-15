using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using DeadLockApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Org.Apache.Http.Protocol;

namespace DeadLockApp.ViewModels
{
    public class BuildEditViewModel : BaseViewModel
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
        bool isLoaded;
        int buildID;
        public static Item SelectedItem { get; set; }

        public BuildEditViewModel()
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
            SelectedItem = null; // Сбрасываем перед открытием

            await Shell.Current.GoToAsync($"{nameof(ItemSelectionPage)}?partId={partId}");

            if (SelectedItem != null)
            {
                AddItemToBuild(SelectedItem, partId);
            }
        }

        public void AddItemToBuild(Item item, int partId)
        {
            Debug.WriteLine($"Добавлен предмет: {item.Name} с картинкой {item.Image} в категорию {partId}");
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
        
        public async Task LoadBuildData(Build build)
        {
            if (isLoaded == false)
            {
                buildID = build.Id;
                BuildName = build.Name;
                isLoaded= true;
            Debug.WriteLine("Загрузка данных билда...");

            foreach (var buildItem in build.Items)
            {
                var item = await GetItemById(buildItem.ItemId); // Дожидаемся получения предмета

                if (item != null)
                {
                    item.Image = $"http://192.168.2.20/public/storage/{item.Image}"; ;
                    Debug.WriteLine($"Загружен предмет: {item.Name} с картинкой {item.Image} в категорию {buildItem.PartId}");
                    switch (buildItem.PartId)
                    {
                        case 1: StartItems.Add(item); break;
                        case 2: MiddleItems.Add(item); break;
                        case 3: EndItems.Add(item); break;
                        case 4: SituationalItems.Add(item); break;
                    }
                }
            }

            OnPropertyChanged(nameof(StartItems));
            OnPropertyChanged(nameof(MiddleItems));
            OnPropertyChanged(nameof(EndItems));
            OnPropertyChanged(nameof(SituationalItems));
            }
        }


        // Метод для получения предмета по его ID
        private async Task<Item> GetItemById(int itemId)
        {
            var allItems = await GetAllItems(); // Дожидаемся завершения задачи
            return allItems.FirstOrDefault(i => i.Id == itemId);
        }

        private static readonly HttpClient _httpClient = new HttpClient();
        public async Task<List<Item>> GetAllItems()
        {
            // Адрес API для получения предметов
            string url = "http://192.168.2.20/api/items";

            try
            {
                // Отправляем GET запрос
                var response = await _httpClient.GetStringAsync(url);

                // Десериализуем JSON-ответ в объект ApiResponse2
                var apiResponse = JsonSerializer.Deserialize<ApiResponse2>(response);

                // Проверяем, если предметы существуют, возвращаем их
                if (apiResponse?.Предметы != null)
                {
                    return apiResponse.Предметы;
                }
                else
                {
                    // Если предметы не найдены, возвращаем пустой список
                    return new List<Item>();
                }
            }
            catch (HttpRequestException e)
            {
                // Логируем или обрабатываем ошибки запроса
                Console.WriteLine($"Request error: {e.Message}");
                return new List<Item>(); // Возвращаем пустой список в случае ошибки
            }
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
                    character_id = Data.CurrentCharacter.Id, 
                    items = GetItemsList()
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(
                    JsonSerializer.Serialize(buildRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync($"http://192.168.2.20/api/builds/{buildID}", content);
                Debug.WriteLine($"Ссылка http://192.168.2.20/api/builds/{buildID}");
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Успех", "Билд сохранен!", "Ок");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка сохранения билда: {responseContent}", "Ок");
                    Debug.WriteLine($"Ошибка сохранения билда: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка запроса: {ex.Message}", "Ок");
                Debug.WriteLine($"Ошибка запроса: {ex.Message}");
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
