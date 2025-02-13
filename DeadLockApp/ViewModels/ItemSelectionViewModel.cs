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
    public class ItemSelectionViewModel : INotifyPropertyChanged
    {
        private const string ItemsApiUrl = "http://192.168.2.20/api/items"; // URL для API, который предоставляет данные о предметах

        // Коллекции для предметов разных категорий
        public ObservableCollection<Item> TierOneItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierTwoItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierThreeItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierFourItems { get; set; } = new ObservableCollection<Item>();

        private List<Item> _items = new List<Item>(); // Все предметы

        public List<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items)); // Уведомляем об изменении
                }
            }
        }

        public ICommand ChangeCategoryCommand { get; } // Команда для изменения категории
        private string _selectedCategory; // Выбранная категория
        public ICommand SelectItemCommand { get; }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory)); // Уведомляем об изменении категории
                }
            }
        }

        // Конструктор, инициализирующий команду и загружающий данные
        public ItemSelectionViewModel() 
        {
            SelectItemCommand = new Command<Item>(SelectItem);
            ChangeCategoryCommand = new Command<string>(ChangeCategory); // Инициализация команды изменения категории
            LoadItemsAsync(); // Загрузка предметов при инициализации
        }

        // Асинхронный метод для получения данных о предметах с API
        public async Task<List<Item>> FetchItemsAsync()
        {
            try
            {
                var client = new HttpClient(); // Инициализация HTTP клиента
                var response = await client.GetStringAsync(ItemsApiUrl); // Получение данных с API
                var data = JsonSerializer.Deserialize<ApiResponse2>(response); // Десериализация ответа
                return data?.Предметы ?? new List<Item>(); // Возвращаем список предметов или пустой список
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}"); // Логирование ошибки
                return new List<Item>(); // Возвращаем пустой список при ошибке
            }
        }

        // Метод для изменения категории
        private void ChangeCategory(string category)
        {
            SelectedCategory = category; // Устанавливаем выбранную категорию
            UpdateFilteredItems(category); // Обновляем отображаемые предметы
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
                "Weapon" => 1, // Оружие
                "Vitality" => 2, // Жизненная сила
                "Spirit" => 3, // Дух
                _ => 0, // По умолчанию
            };
        }

        // Асинхронный метод для загрузки предметов и их обработки
        public async void LoadItemsAsync()
        {
            var items = await FetchItemsAsync(); // Получаем предметы с API

            // Устанавливаем изображение для каждого предмета
            foreach (var item in items)
            {
                item.Image = $"http://192.168.2.20/public/storage/{item.Image}";
            }

            Items = items; // Заполняем список предметов
            UpdateFilteredItems("Weapon"); // Обновляем отображаемые предметы по умолчанию (Оружие)
        }

        public event PropertyChangedEventHandler PropertyChanged; // Событие для уведомления об изменении свойств

        // Метод для уведомления об изменении свойства
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Вызываем событие изменения свойства
        }
        private async void SelectItem(Item item)
        {
            if (item == null) return;

            if (Shell.Current.CurrentPage is ItemSelectionPage selectionPage)
            {
                int partId = selectionPage.PartId;

                // Получаем ViewModel страницы создания билда
                if (Shell.Current.Navigation.NavigationStack.FirstOrDefault(p => p is BuildCreatePage) is BuildCreatePage buildPage)
                {
                    if (buildPage.BindingContext is BuildCreateViewModel buildViewModel)
                    {
                        buildViewModel.AddItemToBuild(item, partId);
                    }
                }
            }

            await Shell.Current.Navigation.PopAsync(); // Закрываем страницу выбора предмета
        }
    }

}
