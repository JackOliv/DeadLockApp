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
        // URL для API, который предоставляет информацию о билдах
        private const string ApiUrl = "http://192.168.2.20/api/character/{0}/builds"; 
        // Коллекция билдов для привязки
        public ObservableCollection<Build> Builds { get; set; } = new(); 
        // Выбранный билд
        public Build SelectedBuild { get; set; } 
        // Команда для создания нового билда
        public ICommand CreateBuildCommand { get; } 
        public BuildsViewModel()
        {
            // Инициализация команды для создания билда
            CreateBuildCommand = new Command(CreateBuild); 
        }
        // Асинхронный метод для загрузки билдов из API по ID персонажа
        public async Task LoadBuildsAsync(int characterId)
        {
            try
            {
                // Формируем URL для API-запроса
                string url = string.Format(ApiUrl, characterId); 
                // Инициализируем HTTP клиент
                using HttpClient client = new(); 
                // Получаем строку ответа от API
                string response = await client.GetStringAsync(url); 
                // Десериализуем данные в объект BuildResponse
                var data = JsonSerializer.Deserialize<BuildResponse>(response); 
                // Проверяем, что данные были успешно загружены
                if (data != null && data.Builds != null) 
                {
                    // Очистить коллекцию перед загрузкой новых данных
                    Debug.WriteLine($"Loaded {Builds.Count} builds before adding new data.");
                    // Очищаем текущие данные в коллекции билдов
                    Builds.Clear(); 
                    // Добавить только новые данные, проверяя на дубли
                    foreach (var build in data.Builds)
                    {
                        // Проверка на дубли по ID билда
                        if (!Builds.Any(b => b.Id == build.Id)) 
                        {
                            Debug.WriteLine($"Adding build: {build.Name}");
                            // Добавляем билд в коллекцию
                            Builds.Add(build); 
                        }
                    }
                    Debug.WriteLine($"Loaded {Builds.Count} builds after adding data.");
                }
                else
                {
                    // Логирование при отсутствии данных
                    Debug.WriteLine("No builds found or failed to parse data."); 
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки при загрузке данных
                Debug.WriteLine($"Error in LoadBuildsAsync: {ex.Message}"); 
            }
        }
        // Событие для уведомления об изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;
        // Метод для уведомления об изменении свойства
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // Вызываем событие изменения свойства
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
        // Метод для создания нового билда
        private void CreateBuild()
        {
            Debug.WriteLine("CreateBuild command executed.");
        }
    }
}
