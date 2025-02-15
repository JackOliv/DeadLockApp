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
    // ViewModel для страницы выбора предметов
    public class ItemSelectionViewModel : INotifyPropertyChanged
    {
        // URL API для получения предметов
        private const string ItemsApiUrl = "http://192.168.2.20/api/items";

        // Коллекции для предметов разных категорий (по уровням)
        public ObservableCollection<Item> TierOneItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierTwoItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierThreeItems { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Item> TierFourItems { get; set; } = new ObservableCollection<Item>();

        // Лист для хранения всех предметов
        private List<Item> _items = new List<Item>();

        // Свойство для доступа к списку всех предметов
        public List<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items)); // Уведомление об изменении свойства
                }
            }
        }

        // Команды для изменения категории и выбора предмета
        public ICommand ChangeCategoryCommand { get; }
        public ICommand SelectItemCommand { get; }

        private string _selectedCategory; // Свойство для выбранной категории
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory)); // Уведомление об изменении категории
                }
            }
        }

        // Конструктор, который инициализирует команды и загружает предметы
        public ItemSelectionViewModel()
        {
            SelectItemCommand = new Command<Item>(SelectItem); // Инициализация команды выбора предмета
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
                var data = JsonSerializer.Deserialize<ApiResponse2>(response); // Десериализация ответа в объект
                return data?.Предметы ?? new List<Item>(); // Возвращаем список предметов или пустой список в случае ошибки
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}"); // Логирование ошибки
                return new List<Item>(); // Возвращаем пустой список при ошибке
            }
        }

        // Метод для изменения категории и обновления отображаемых предметов
        private void ChangeCategory(string category)
        {
            SelectedCategory = category; // Устанавливаем выбранную категорию
            UpdateFilteredItems(category); // Обновляем список отображаемых предметов
        }

        // Метод для обновления списка предметов в зависимости от выбранной категории
        private void UpdateFilteredItems(string category)
        {
            // Очищаем коллекции для разных категорий
            TierOneItems.Clear();
            TierTwoItems.Clear();
            TierThreeItems.Clear();
            TierFourItems.Clear();

            // Добавляем предметы в соответствующие коллекции в зависимости от их уровня и типа
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

            // Уведомляем об изменении всех коллекций
            OnPropertyChanged(nameof(TierOneItems));
            OnPropertyChanged(nameof(TierTwoItems));
            OnPropertyChanged(nameof(TierThreeItems));
            OnPropertyChanged(nameof(TierFourItems));
        }

        // Метод для получения идентификатора типа предмета на основе выбранной категории
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
            var items = await FetchItemsAsync(); // Получаем список предметов с API

            // Устанавливаем URL изображения для каждого предмета
            foreach (var item in items)
            {
                item.Image = $"http://192.168.2.20/public/storage/{item.Image}"; // Формируем полный путь к изображению
            }

            Items = items; // Заполняем список всех предметов
            UpdateFilteredItems("Weapon"); // По умолчанию отображаем предметы категории "Оружие"
        }

        // Событие для уведомления об изменении свойств
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод для вызова события изменения свойства
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Уведомляем подписчиков
        }

        // Метод для выбора предмета и добавления его в билд
        private async void SelectItem(Item item)
        {
            if (item == null) return;

            // Проверяем текущую страницу и находим нужную часть билда (через PartId)
            if (Shell.Current.CurrentPage is ItemSelectionPage selectionPage)
            {
                int partId = selectionPage.PartId;

                // Добавляем предмет в билд на странице создания билда
                if (Shell.Current.Navigation.NavigationStack.FirstOrDefault(p => p is BuildCreatePage) is BuildCreatePage buildPage)
                {
                    if (buildPage.BindingContext is BuildCreateViewModel buildViewModel)
                    {
                        buildViewModel.AddItemToBuild(item, partId); // Добавляем предмет в билд
                    }
                }

                // Добавляем предмет в билд на странице редактирования билда
                if (Shell.Current.Navigation.NavigationStack.FirstOrDefault(p => p is BuildEditPage) is BuildEditPage editPage)
                {
                    if (editPage.BindingContext is BuildEditViewModel buildViewModel)
                    {
                        buildViewModel.AddItemToBuild(item, partId); // Добавляем предмет в билд
                    }
                }
            }

            // Закрываем страницу выбора предмета
            await Shell.Current.GoToAsync("..");

        }
    }
}
