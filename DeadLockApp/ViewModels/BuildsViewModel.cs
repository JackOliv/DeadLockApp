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
    public class BuildsViewModel : BaseViewModel
    {
        private const string ApiUrl = "http://192.168.2.20/api/character/{0}/builds"; // URL для API, который предоставляет информацию о билдах

        public ObservableCollection<Build> Builds { get; set; } = new(); // Коллекция билдов для привязки
        public Build SelectedBuild { get; set; } // Выбранный билд
        public ICommand CreateBuildCommand { get; } // Команда для создания нового билда

        public BuildsViewModel()
        {
            CreateBuildCommand = new Command(CreateBuild); // Инициализация команды для создания билда
        }

        // Асинхронный метод для загрузки билдов из API по ID персонажа
        public async Task LoadBuildsAsync(int characterId)
        {
            try
            {
                string url = string.Format(ApiUrl, characterId); // Формируем URL для API-запроса

                using HttpClient client = new(); // Инициализируем HTTP клиент
                string response = await client.GetStringAsync(url); // Получаем строку ответа от API

                var data = JsonSerializer.Deserialize<BuildResponse>(response); // Десериализуем данные в объект BuildResponse

                if (data != null && data.Builds != null) // Проверяем, что данные были успешно загружены
                {
                    // Очистить коллекцию перед загрузкой новых данных
                    Debug.WriteLine($"Loaded {Builds.Count} builds before adding new data.");
                    Builds.Clear(); // Очищаем текущие данные в коллекции билдов

                    // Добавить только новые данные, проверяя на дубли
                    foreach (var build in data.Builds)
                    {
                        if (!Builds.Any(b => b.Id == build.Id)) // Проверка на дубли по ID билда
                        {
                            Debug.WriteLine($"Adding build: {build.Name}");
                            Builds.Add(build); // Добавляем билд в коллекцию
                        }
                    }
                    Debug.WriteLine($"Loaded {Builds.Count} builds after adding data.");
                }
                else
                {
                    Debug.WriteLine("No builds found or failed to parse data."); // Логирование при отсутствии данных
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadBuildsAsync: {ex.Message}"); // Логирование ошибки при загрузке данных
            }
        }

        // Событие для уведомления об изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод для уведомления об изменении свойства
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // Вызываем событие изменения свойства
        }

        // Метод для создания нового билда
        private void CreateBuild()
        {
            Debug.WriteLine("CreateBuild command executed."); // Логирование вызова команды
            // Реализуйте переход на страницу создания билдов или логику
        }
    }
}
