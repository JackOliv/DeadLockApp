using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using DeadLockApp.Models;
using System.Diagnostics;

namespace DeadLockApp.ViewModels
{
    public class ItemDetailsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Image));
                OnPropertyChanged(nameof(Cost));
                OnPropertyChanged(nameof(FullImagePath));
            }
        }
        public string FullImagePath => SelectedItem != null ? $"http://192.168.2.20/storage/{SelectedItem.Image}" : string.Empty;
        public string Name => SelectedItem?.Name;
        public string Description => SelectedItem?.Description;
        public string Image => SelectedItem?.Image;
        public int Cost => SelectedItem?.Cost ?? 0;
        public async Task InitializeAsync(int itemId)
        {
            await LoadItemDetails(itemId);
        }

        public ItemDetailsViewModel() { }

        public async Task LoadItemDetails(int itemId)
        {
            Debug.WriteLine($"Загружаю предмет с ID: {itemId}");

            var allItems = await new ItemsViewModel().FetchItemsAsync();
            SelectedItem = allItems.FirstOrDefault(i => i.Id == itemId);
            if (SelectedItem == null)
            {
                Debug.WriteLine("Предмет не найден!");
            }
        }
    }
}
