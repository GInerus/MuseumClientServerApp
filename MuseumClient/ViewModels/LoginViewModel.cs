using MuseumClient.Commands;
using MuseumClient.Helpers;
using MuseumClient.Models;
using MuseumClient.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainVM;
        private readonly AuthService _authService;
        private readonly ConfigService _configService;

        public RelayCommand RegisterCommand { get; }
        public RelayCommand SaveServerSettingsCommand { get; }

        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserPassword)));
                }
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(ErrorMessage)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPasswordVisible)));
                }
            }
        }

        public RelayCommand TogglePasswordVisibilityCommand { get; }

        public List<UserTypeItem> UserTypes { get; } = new()
        {
            new UserTypeItem { Display = "Гость", Value = "guest" },
            new UserTypeItem { Display = "Администратор", Value = "admin" }
        };

        public bool IsPasswordRequired => SelectedUserType?.Value == "admin";

        public class UserTypeItem
        {
            public string Display { get; set; }
            public string Value { get; set; }
        }

        public string UserType { get; set; }

        private UserTypeItem _selectedUserType;
        public UserTypeItem SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value;

                ErrorMessage = string.Empty;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedUserType)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPasswordRequired)));
            }
        }

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;


            SelectedUserType = UserTypes.First();

            // Инициализация Singleton AuthService с конфигом
            _configService = new ConfigService();
            AuthService.Instance(_configService.Server);

            TogglePasswordVisibilityCommand = new RelayCommand(async _ =>
            {
                IsPasswordVisible = !IsPasswordVisible;
                await Task.CompletedTask;
            });

            RegisterCommand = new RelayCommand(async _ =>
            {
                var result = await AuthService.Instance().RegisterAsync(
                    SelectedUserType.Value,
                    SelectedUserType.Value == "guest" ? " " : UserPassword);

                switch (result)
                {
                    case AuthResult.Success:

                        ErrorMessage = "";
                        _mainVM.ShowContentHubView();
                        break;

                    case AuthResult.InvalidCredentials:

                        ErrorMessage = "Неверный пароль.";
                        break;

                    case AuthResult.ServerUnavailable:

                        ErrorMessage = "Нет доступа к серверу.";
                        break;
                }
            });

            SaveServerSettingsCommand = new RelayCommand(async _ =>
            {
                if (!Uri.TryCreate(LocalUrl, UriKind.Absolute, out var localUri) ||
                    (localUri.Scheme != Uri.UriSchemeHttp && localUri.Scheme != Uri.UriSchemeHttps))
                {
                    InfoService.Show("Некорректный адрес локального сервера.");
                    return;
                }

                if (!Uri.TryCreate(RemoteUrl, UriKind.Absolute, out var remoteUri) ||
                    (remoteUri.Scheme != Uri.UriSchemeHttp && remoteUri.Scheme != Uri.UriSchemeHttps))
                {
                    InfoService.Show("Некорректный адрес удалённого сервера.");
                    return;
                }

                if (_configService.Save())
                {
                    AuthService.Instance().UpdateServerConfig(_configService.Server);

                    InfoService.Show("Адреса серверов сохранены.");
                }
                else
                {
                    try
                    {
                        _configService.SaveWithAdministrator();

                        InfoService.Show(
                            "Для сохранения настроек необходимо подтвердить запрос администратора.");
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        InfoService.Show("Сохранение отменено.");
                    }
                }

                await Task.CompletedTask;
            });

            ToggleServerSettingsCommand = new RelayCommand(async _ =>
            {
                ShowServerSettings = !ShowServerSettings;
                await Task.CompletedTask;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string LocalUrl
        {
            get => _configService.Server.LocalUrl;
            set
            {
                _configService.Server.LocalUrl = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(LocalUrl)));
            }
        }

        public string RemoteUrl
        {
            get => _configService.Server.RemoteUrl;
            set
            {
                _configService.Server.RemoteUrl = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(RemoteUrl)));
            }
        }

        private bool _showServerSettings;
        public bool ShowServerSettings
        {
            get => _showServerSettings;
            set
            {
                _showServerSettings = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(ShowServerSettings)));
            }
        }

        public RelayCommand ToggleServerSettingsCommand { get; }
    }
}