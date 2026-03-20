using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MeseumClient.Commands;
using MeseumClient.Services;

namespace MeseumClient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly SessionService _sessionService;

        public LoginViewModel(SessionService sessionService)
        {
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            SelectedUserType = "guest"; // сразу инициализируем
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        }

        // Инициализация коллекции прямо в объявлении
        public ObservableCollection<string> UserTypes { get; } = new() { "guest", "admin" };

        private string _selectedUserType = string.Empty;
        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value ?? string.Empty;
                OnPropertyChanged();

                IsPasswordFieldVisible = value == "admin";
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
            StatusMessage = "Проверка соединения с сервером...";

            bool serverFound = await _sessionService.DiscoverServerAsync();
            if (!serverFound)
            {
                StatusMessage = "Сервер не найден. Попробуйте снова.";
                return;
            }

            StatusMessage = "Авторизация...";

            bool loggedIn = await _sessionService.RegisterSessionAsync(SelectedUserType, Password);
            if (!loggedIn)
            {
                StatusMessage = "Ошибка авторизации. Проверьте пароль.";
                return;
            }

            StatusMessage = string.Empty;
            LoginSucceeded?.Invoke(SelectedUserType);
        }

        public event Action<string?>? LoginSucceeded;
    }
}