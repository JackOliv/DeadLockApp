using System.Text.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;

namespace DeadLockApp.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

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

        private string _passwordConfirmationd;
        public string PasswordConfirmation
        {
            get => _passwordConfirmationd;
            set
            {
                if (_passwordConfirmationd != value)
                {
                    _passwordConfirmationd = value;
                    OnPropertyChanged(nameof(PasswordConfirmation));
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

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
        }

        private async Task RegisterAsync()
        {
            IsErrorVisible = false;

            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(PasswordConfirmation))
            {
                ErrorMessage = "Пожалуйста, заполните все поля.";
                IsErrorVisible = true;
                return;
            }

            if (Username.Length < 3)
            {
                ErrorMessage = "Логин должен содержать не менее 3 символов.";
                IsErrorVisible = true;
                return;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Пароль должен содержать не менее 8 символов.";
                IsErrorVisible = true;
                return;
            }

            if (Password != PasswordConfirmation)
            {
                ErrorMessage = "Пароли не совпадают.";
                IsErrorVisible = true;
                return;
            }

            var isSuccess = await RegisterUserAsync(Name, Username, Password);
            if (isSuccess)
            {
                await Shell.Current.GoToAsync(".."); // Возвращение на страницу логина
            }
            else
            {
                IsErrorVisible = true;
            }
        }

        private async Task<bool> RegisterUserAsync(string name, string login, string password)
        {
            try
            {
                using var httpClient = new HttpClient();
                var url = "http://192.168.2.20/public/api/register";

                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        name,
                        login,
                        password,
                        password_confirmation = PasswordConfirmation
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"Response StatusCode: {response.StatusCode}");
                Debug.WriteLine($"Response Content: {responseContent}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка подключения: {ex.Message}";
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }
    }
}