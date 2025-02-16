using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DeadLockApp.Models;

namespace DeadLockApp.ViewModels
{
    public class HeroesViewModel : BaseViewModel
    {
        private const string CharactersApiUrl = "http://192.168.2.20/api/characters";
        // Инициализация HTTP клиента
        private readonly HttpClient _httpClient = new HttpClient(); 
        // Коллекция персонажей для привязки
        public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>(); 
        // Выбранный персонаж
        public Character SelectedCharacter { get; set; } 
        public HeroesViewModel()
        {
            // Загружаем персонажей асинхронно при инициализации ViewModel
            _ = LoadCharactersAsync(); 
        }
        // Асинхронный метод для получения списка персонажей из API
        private async Task<List<Character>> FetchCharactersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Получаем строку ответа от API
                var response = await _httpClient.GetStringAsync(CharactersApiUrl, cancellationToken);
                // Десериализуем ответ
                var data = JsonSerializer.Deserialize<ApiResponse>(response);
                // Возвращаем список персонажей, если он есть, или пустой список
                return data?.Персонажи ?? new List<Character>(); 
            }
            catch (Exception ex)
            {
                // Логирование ошибки при получении данных
                Debug.WriteLine($"Error fetching characters: {ex.Message}"); 
                // Возвращаем пустой список в случае ошибки
                return new List<Character>(); 
            }
        }
        // Асинхронный метод для загрузки и обработки персонажей
        private async Task LoadCharactersAsync()
        {
            try
            {
                // Загружаем персонажей
                var characters = await FetchCharactersAsync();
                // Проверка на успешную загрузку персонажей
                if (characters != null && characters.Any()) 
                {
                    // Очищаем текущий список персонажей
                    Characters.Clear(); 
                    foreach (var character in characters)
                    {
                        // Формируем полный путь для изображения
                        character.Image = $"http://192.168.2.20/storage/{character.Image}";
                        // Добавляем персонажа в коллекцию
                        Characters.Add(character);
                        // Логирование загруженного персонажа
                        Debug.WriteLine($"Loaded: {character.Image}"); 
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки при загрузке персонажей
                Debug.WriteLine($"Error loading characters: {ex.Message}"); 
            }
        }
        // Метод для сброса выбранного персонажа
        public void ResetSelectedCharacter()
        {
            SelectedCharacter = null; // Сбрасываем выбор персонажа
        }
        // Событие изменения свойства для привязки данных
        public event PropertyChangedEventHandler PropertyChanged;
        // Метод для уведомления об изменении свойства
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Вызываем событие изменения свойства
        }
    }
}
