using System.Text.Json;
using System.Windows.Input;
using System.Text; // Подключаем пространство имён для Encoding
using System.Net.Http;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Text.Json.Serialization;
namespace DeadLockApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }


        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        private bool _isErrorVisible;
        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set
            {
                if (_isErrorVisible != value)
                {
                    _isErrorVisible = value;
                    OnPropertyChanged(nameof(IsErrorVisible));
                }
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            IsErrorVisible = false;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Пожалуйста, введите логин и пароль.";
                IsErrorVisible = true;
                return;
            }

            // Пример запроса к API
            var isSuccess = await AuthenticateUserAsync(Username, Password);
            if (isSuccess)
            {
                // Перенаправление на BuildCreatePage
                await Shell.Current.GoToAsync(nameof(BuildCreatePage)); // Переход на BuildCreatePage
                IsErrorVisible = true;
                ErrorMessage = "Успешно вошел";
            }
            else
            {
                IsErrorVisible = true;
            }
        }

        private async Task<bool> AuthenticateUserAsync(string login, string password)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = "http://192.168.2.20/public/api/login";

                // Формируем тело запроса
                var content = new StringContent(
                    JsonSerializer.Serialize(new { login, password }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Отправляем POST-запрос
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Логирование для диагностики
                Debug.WriteLine($"Response StatusCode: {response.StatusCode}");
                Debug.WriteLine($"Response ReasonPhrase: {response.ReasonPhrase}");
                Debug.WriteLine($"Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    // Десериализация ответа
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    // Логирование результата десериализации
                    Debug.WriteLine($"Result: {JsonSerializer.Serialize(result)}");

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        // Сохраняем токен и роль
                        await SecureStorage.SetAsync("auth_token", result.Token);
                        await SecureStorage.SetAsync("role_code", result.User.RoleCode); // Сохраняем role_code
                        await SecureStorage.SetAsync("username", result.User.Name); // Сохраняем имя пользователя

                        return true;
                    }
                    else
                    {
                        // Если токен или данные пользователя отсутствуют
                        ErrorMessage = "Ошибка: Токен или данные пользователя отсутствуют в ответе.";
                        Debug.WriteLine("Ошибка: Токен или данные пользователя отсутствуют в ответе.");
                    }
                }
                else
                {
                    // Если статус ответа не успешный
                    ErrorMessage = "Ошибка: Статус-код ответа не успешный.";
                    Debug.WriteLine("Ошибка: Статус-код ответа не успешный.");
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подключения: {ex.Message}";
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }



        // Класс для десериализации ответа
        // Класс для десериализации ответа
        public class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("user")]
            public UserDetails User { get; set; } // Изменено на UserDetails для предотвращения конфликта

            public class UserDetails // Переименовано из User
            {
                [JsonPropertyName("name")]
                public string Name { get; set; } // Имя пользователя

                [JsonPropertyName("role_code")]
                public string RoleCode { get; set; } // Роль пользователя
            }
        }


    }
}