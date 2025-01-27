using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DeadLockApp.Models;

namespace DeadLockApp.ViewModels
{
    public class BuildDetailsViewModel : BaseViewModel
    {
        private const string BuildDetailsApiUrl = "http://course-project-4/api/character/"; // URL для API

        public ObservableCollection<Item> StartItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> MiddleItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> EndItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> SituationsItems { get; set; } = new ObservableCollection<Item>();

        private string _buildName;
        private string _buildAuthor;
        public string BuildName
        {
            get => _buildName;
            set
            {
                _buildName = value;
                OnPropertyChanged(nameof(BuildName));
            }
        }
        public string BuildAuthor
        {
            get => _buildAuthor;
            set
            {
                _buildAuthor = value;
                OnPropertyChanged(nameof(BuildAuthor));
            }
        }

        public async Task LoadBuildDetailsAsync(int buildId, int characterId)
        {
            try
            {
                Debug.WriteLine(buildId);
                var client = new HttpClient();
                var response = await client.GetStringAsync($"{BuildDetailsApiUrl}{characterId}/builds");

                Debug.WriteLine("API Response:");
                Debug.WriteLine(response);  // Отладочный вывод полученного ответа

                var data = JsonSerializer.Deserialize<BuildResponse>(response);

                if (data != null && data.Builds != null && data.Builds.Any())
                {
                    Debug.WriteLine($"Number of builds found: {data.Builds.Count}");

                    // Ищем нужный билд по ID
                    var build = data.Builds.FirstOrDefault(b => b.Id == buildId);
                    if (build != null)
                    {
                        Debug.WriteLine($"Selected Build Name: {build.Name}");

                        BuildName = build.Name;
                        BuildAuthor = build.Author;

                        StartItems.Clear();
                        MiddleItems.Clear();
                        EndItems.Clear();
                        SituationsItems.Clear();

                        var items = await GetItemsAsync();
                        Debug.WriteLine($"Total items fetched: {items.Count}");

                        foreach (var item in build.Items)
                        {
                            Debug.WriteLine($"Processing Item ID: {item.ItemId}, Part ID: {item.PartId}");

                            // Поиск деталей предмета по ItemId
                            var itemDetail = items.FirstOrDefault(i => i.Id == item.ItemId);
                            if (itemDetail != null)
                            {
                                Console.WriteLine($"Item found: {itemDetail.Name}, assigning to Part ID: {item.PartId}");
                                itemDetail.Image = $"http://course-project-4/public/storage/{itemDetail.Image}";
                                switch (item.PartId)
                                {
                                    case 1: StartItems.Add(itemDetail); break;
                                    case 2: MiddleItems.Add(itemDetail); break;
                                    case 3: EndItems.Add(itemDetail); break;
                                    case 4: SituationsItems.Add(itemDetail); break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Item ID {item.ItemId} not found in fetched items.");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"No build found with ID: {buildId}");
                    }
                }
                else
                {
                    Debug.WriteLine("No builds found or data is null.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading build details: {ex.Message}");
            }
        }


        private async Task<List<Item>> GetItemsAsync()
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync("http://course-project-4/api/items"); // URL для API
            Debug.WriteLine("Items API Response:");
            Debug.WriteLine(response); // Отладочный вывод ответа

            try
            {
                // Десериализуем ApiResponse2, чтобы учесть корневое поле "Предметы"
                var apiResponse = JsonSerializer.Deserialize<ApiResponse2>(response);

                if (apiResponse != null && apiResponse.Предметы != null)
                {
                    Debug.WriteLine($"Total items fetched: {apiResponse.Предметы.Count}");
                    return apiResponse.Предметы;
                }
                else
                {
                    Debug.WriteLine("No items found or response is null.");
                    return new List<Item>();
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return new List<Item>();
            }
        }


    }
}
