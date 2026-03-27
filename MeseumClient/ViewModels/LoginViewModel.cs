using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MeseumClient.Commands;
using MeseumClient.Services;

namespace MeseumClient.ViewModels
{
    // Модель для выпадающего списка
    public class UserTypeOption
    {
        public string Value { get; set; } = string.Empty;   // для логики и сервера
        public string Display { get; set; } = string.Empty; // для UI
    }

    public class LoginViewModel : BaseViewModel
    {
        private readonly SessionService _sessionService;

        public LoginViewModel(SessionService sessionService)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            // Инициализация команд
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);

            // Инициализация списка пользователей
            UserTypes = new ObservableCollection<UserTypeOption>
            {
                new UserTypeOption { Value = "guest", Display = "Гость" },
                new UserTypeOption { Value = "admin", Display = "Администратор" }
            };

            // Выбор по умолчанию
            SelectedUserType = UserTypes[0];
        }

        // Список для ComboBox
        public ObservableCollection<UserTypeOption> UserTypes { get; }

        private UserTypeOption? _selectedUserType;
        public UserTypeOption? SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value;
                OnPropertyChanged();

                IsPasswordFieldVisible = value?.Value == "admin";
                if (!IsPasswordFieldVisible) Password = string.Empty;
                IsPasswordVisible = false;
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isPasswordFieldVisible;
        public bool IsPasswordFieldVisible
        {
            get => _isPasswordFieldVisible;
            set
            {
                _isPasswordFieldVisible = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        private async Task LoginAsync()
        {
            StatusMessage = "Авторизация...";

            // Отправляем серверу английское значение
            bool loggedIn = await _sessionService.RegisterSessionAsync(
                SelectedUserType?.Value ?? "guest",
                Password
            );

            if (!loggedIn)
            {
                StatusMessage = "Ошибка авторизации. Проверьте пароль.";
                return;
            }

            StatusMessage = string.Empty;
            LoginSucceeded?.Invoke(_sessionService.Token);
        }

        public event Action<string?>? LoginSucceeded;
    }
}