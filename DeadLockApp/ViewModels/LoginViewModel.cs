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
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
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

                var content = new StringContent(
                    JsonSerializer.Serialize(new { login, password }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Response StatusCode: {response.StatusCode}");
                Debug.WriteLine($"Response Content: {responseContent}");

                var result = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ErrorMessage = "Ошибка: Некорректный ответ сервера.";
                    Debug.WriteLine(ErrorMessage);
                    return false;
                }

                await SecureStorage.SetAsync("auth_token", result.Token);

                if (result.User != null) // Проверяем, есть ли user
                {
                    await SecureStorage.SetAsync("role_code", result.User.RoleCode ?? "");
                    await SecureStorage.SetAsync("username", result.User.Name ?? "");
                    var testValue = await SecureStorage.GetAsync("username");
                    Debug.WriteLine($"SecureStorage Test Value: {testValue}");
                    Debug.WriteLine($"User.Name: {result.User?.Name}");
                    Debug.WriteLine($"User.RoleCode: {result.User?.RoleCode}");
                    await SecureStorage.SetAsync("test_key", "test_value");
                    var storedValue = await SecureStorage.GetAsync("test_key");
                    Debug.WriteLine($"Stored test_key: {storedValue}");

                }
                else
                {
                    Debug.WriteLine($"Penis");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подключения: {ex.Message}";
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }

        // Класс для десериализации ответа
        public class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("user")]
            public UserDetails? User { get; set; } // Теперь может быть null

            public class UserDetails
            {
                [JsonPropertyName("name")]
                public string Name { get; set; }

                [JsonPropertyName("role_code")]
                public string RoleCode { get; set; }
            }
        }


    }
}