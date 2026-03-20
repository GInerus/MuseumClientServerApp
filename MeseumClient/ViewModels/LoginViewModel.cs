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

            UserTypes = new ObservableCollection<string> { "guest", "admin" };

            // Инициализация полей для подавления CS8618
            _selectedUserType = string.Empty;
            _password = string.Empty;
            _statusMessage = string.Empty;

            SelectedUserType = "guest"; // Это вызовет Setter и обновит IsPasswordFieldVisible

            LoginCommand = new RelayCommand(async () => await LoginAsync());
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        }

        public ObservableCollection<string> UserTypes { get; }

        private string _selectedUserType;
        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                if (_selectedUserType != value)
                {
                    _selectedUserType = value ?? string.Empty;
                    OnPropertyChanged();

                    // Обновляем видимость поля пароля при смене типа пользователя
                    IsPasswordFieldVisible = value == "admin";

                    // Очищаем пароль при смене пользователя
                    if (!IsPasswordFieldVisible)
                    {
                        Password = string.Empty;
                    }

                    // Сбрасываем состояние видимости пароля
                    IsPasswordVisible = false;
                }
            }
        }

        private string _password = string.Empty;
        private string _tempPassword = string.Empty;

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value ?? string.Empty;
                    _tempPassword = _password; // Синхронизируем с временным полем
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;

                    // При переключении видимости копируем значение из временного поля
                    if (_isPasswordVisible)
                    {
                        // Переключаемся на TextBox - копируем пароль
                        Password = _tempPassword;
                    }
                    else
                    {
                        // Переключаемся на PasswordBox - сохраняем в временное поле
                        _tempPassword = Password;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        // Свойство для управления видимостью поля пароля (показывать/скрывать полностью)
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

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task LoginAsync()
        {
            StatusMessage = "Проверка соединения с сервером...";

            bool serverFound = await _sessionService.DiscoverServerAsync();
            if (!serverFound)
            {
                StatusMessage = "Нет соединения с сервером. Попробуйте снова.";
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
            OnLogin();
        }

        public event Action<string?>? LoginSucceeded;

        private void OnLogin()
        {
            LoginSucceeded?.Invoke(SelectedUserType);
        }
    }
}