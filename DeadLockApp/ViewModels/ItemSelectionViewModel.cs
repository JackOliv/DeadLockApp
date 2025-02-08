using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using DeadLockApp.Models;

namespace DeadLockApp.ViewModels
{
    public class ItemSelectionViewModel : INotifyPropertyChanged
    {
        private const string ItemsApiUrl = "http://course-project-4/api/items";

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        private Item _selectedItem;
        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public ICommand SelectItemCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ItemSelectionViewModel()
        {
            SelectItemCommand = new Command<Item>(SelectItem);
            LoadItemsAsync();
        }

        private async void LoadItemsAsync()
        {
            var items = await FetchItemsAsync();
            Items.Clear();
            foreach (var item in items)
            {
                item.Image = $"http://course-project-4/public/storage/{item.Image}";
                Items.Add(item);
            }
        }

        private async Task<List<Item>> FetchItemsAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetStringAsync(ItemsApiUrl);
                var data = JsonSerializer.Deserialize<ApiResponse2>(response);
                return data?.Предметы ?? new List<Item>();
            }
            catch
            {
                return new List<Item>();
            }
        }

        private void SelectItem(Item item)
        {
            SelectedItem = item;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
