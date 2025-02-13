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

        private readonly HttpClient _httpClient = new HttpClient(); // Инициализация HTTP клиента

        public ObservableCollection<Character> Characters { get; set; } = new ObservableCollection<Character>(); // Коллекция персонажей для привязки
        public Character SelectedCharacter { get; set; } // Выбранный персонаж
        public HeroesViewModel()
        {
            _ = LoadCharactersAsync(); // Загружаем персонажей асинхронно при инициализации ViewModel
        }

        // Асинхронный метод для получения списка персонажей из API
        private async Task<List<Character>> FetchCharactersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(CharactersApiUrl, cancellationToken); // Получаем строку ответа от API
                var data = JsonSerializer.Deserialize<ApiResponse>(response); // Десериализуем ответ
                return data?.Персонажи ?? new List<Character>(); // Возвращаем список персонажей, если он есть, или пустой список
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching characters: {ex.Message}"); // Логирование ошибки при получении данных
                return new List<Character>(); // Возвращаем пустой список в случае ошибки
            }
        }

        // Асинхронный метод для загрузки и обработки персонажей
        private async Task LoadCharactersAsync()
        {
            try
            {
                var characters = await FetchCharactersAsync(); // Загружаем персонажей

                if (characters != null && characters.Any()) // Проверка на успешную загрузку персонажей
                {
                    Characters.Clear(); // Очищаем текущий список персонажей
                    foreach (var character in characters)
                    {
                        character.Image = $"http://192.168.2.20/storage/{character.Image}"; // Формируем полный путь для изображения
                        Characters.Add(character); // Добавляем персонажа в коллекцию
                        Debug.WriteLine($"Loaded: {character.Image}"); // Логирование загруженного персонажа
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading characters: {ex.Message}"); // Логирование ошибки при загрузке персонажей
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
