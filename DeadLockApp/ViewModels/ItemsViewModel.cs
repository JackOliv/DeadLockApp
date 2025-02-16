using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using DeadLockApp.Models;
namespace DeadLockApp.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        // URL для API, который предоставляет данные о предметах
        private const string ItemsApiUrl = "http://192.168.2.20/api/items"; 
        // Коллекции для предметов разных категорий
        public ObservableCollection<Item> TierOneItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierTwoItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierThreeItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierFourItems { get; set; } = new ObservableCollection<Item>();
        // Все предметы
        private List<Item> _items = new List<Item>(); 
        public List<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    // Уведомляем об изменении
                    OnPropertyChanged(nameof(Items)); 
                }
            }
        }
        // Команда для изменения категории
        public ICommand ChangeCategoryCommand { get; } 
        public ICommand ShowItemDetailsCommand { get; }
        // Выбранная категория
        private string _selectedCategory; 
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    // Уведомляем об изменении категории
                    OnPropertyChanged(nameof(SelectedCategory)); 
                }
            }
        }
        // Конструктор, инициализирующий команду и загружающий данные
        public ItemsViewModel()
        {
            ShowItemDetailsCommand = new Command<int>(async (itemId) => await OpenItemDetails(itemId));
            // Инициализация команды изменения категории
            ChangeCategoryCommand = new Command<string>(ChangeCategory); 
            // Загрузка предметов при инициализации
            LoadItemsAsync(); 
        }
        // Асинхронный метод для получения данных о предметах с API
        public async Task<List<Item>> FetchItemsAsync()
        {
            try
            {
                // Инициализация HTTP клиента
                var client = new HttpClient(); 
                // Получение данных с API
                var response = await client.GetStringAsync(ItemsApiUrl); 
                // Десериализация ответа
                var data = JsonSerializer.Deserialize<ApiResponse2>(response); 
                // Возвращаем список предметов или пустой список
                return data?.Предметы ?? new List<Item>(); 
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Error loading items: {ex.Message}"); 
                // Возвращаем пустой список при ошибке
                return new List<Item>(); 
            }
        }
        // Метод для изменения категории
        private void ChangeCategory(string category)
        {
            // Устанавливаем выбранную категорию
            SelectedCategory = category; 
            // Обновляем отображаемые предметы
            UpdateFilteredItems(category); 
        }
        // Обновление предметов в зависимости от выбранной категории
        private void UpdateFilteredItems(string category)
        {
            // Очищаем коллекции для предметов разных категорий
            TierOneItems.Clear();
            TierTwoItems.Clear();
            TierThreeItems.Clear();
            TierFourItems.Clear();
            // Добавляем предметы в соответствующие коллекции по их категориям и уровням
            foreach (var item in Items.Where(x => x.Type_id == GetTypeIdFromCategory(category)))
            {
                switch (item.Tier_id)
                {
                    case 1: TierOneItems.Add(item); break;
                    case 2: TierTwoItems.Add(item); break;
                    case 3: TierThreeItems.Add(item); break;
                    case 4: TierFourItems.Add(item); break;
                }
            }
            // Уведомляем об изменении коллекций
            OnPropertyChanged(nameof(TierOneItems));
            OnPropertyChanged(nameof(TierTwoItems));
            OnPropertyChanged(nameof(TierThreeItems));
            OnPropertyChanged(nameof(TierFourItems));
        }
        // Метод для получения идентификатора типа предмета по категории
        private int GetTypeIdFromCategory(string category)
        {
            return category switch
            {
                // Оружие
                "Weapon" => 1, 
                // Жизненная сила
                "Vitality" => 2, 
                // Дух
                "Spirit" => 3, 
                // По умолчанию
                _ => 0, 
            };
        }
        // Асинхронный метод для загрузки предметов и их обработки
        public async void LoadItemsAsync()
        {
            // Получаем предметы с API
            var items = await FetchItemsAsync(); 
            // Устанавливаем изображение для каждого предмета
            foreach (var item in items)
            {
                item.Image = $"http://192.168.2.20/public/storage/{item.Image}";
            }
            // Заполняем список предметов
            Items = items; 
            // Обновляем отображаемые предметы по умолчанию (Оружие)
            UpdateFilteredItems("Weapon"); 
        }
        // Событие для уведомления об изменении свойств
        public event PropertyChangedEventHandler PropertyChanged; 
        // Метод для уведомления об изменении свойства
        protected void OnPropertyChanged(string propertyName)
        {
            // Вызываем событие изменения свойства
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
        private async Task OpenItemDetails(int itemId)
        {
            Debug.WriteLine($"Открываю предмет с ID: {itemId}");
            if (itemId > 0)
            {
                await Shell.Current.GoToAsync($"ItemDetailsPage?itemId={itemId}", true);
            }
        }
    }

}
