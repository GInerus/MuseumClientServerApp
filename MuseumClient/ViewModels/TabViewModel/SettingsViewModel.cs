using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly AuthService _auth;
        private readonly ContentHubViewModel _hub;

        private bool _canEdit;
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                _canEdit = value;
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        // "Menu" | "Password" | "Log" | "System"
        private string _selectedSection = "Menu";
        public string SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged(nameof(SelectedSection));
            }
        }

        // ===== Информация о системе =====

        public string ClientVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return version != null ? version.ToString() : "неизвестно";
            }
        }

        // Подтягивать реальную версию сервера отдельным GET-запросом
        private string _serverVersion = "—";
        public string ServerVersion
        {
            get => _serverVersion;
            set
            {
                _serverVersion = value;
                OnPropertyChanged(nameof(ServerVersion));
            }
        }

        public async Task LoadServerVersionAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<ServerVersionResponse>(
                    "system/version");

                if (response?.Data != null)
                {
                    ServerVersion = response.Data.Version;
                }
                else
                {
                    ServerVersion = "неизвестно";
                }
            }
            catch
            {
                ServerVersion = "недоступно";
            }
        }

        // ===== Смена пароля =====

        private string _oldPassword = "";
        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                OnPropertyChanged(nameof(OldPassword));
            }
        }

        private string _newPassword = "";
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        private string _confirmNewPassword = "";
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                _confirmNewPassword = value;
                OnPropertyChanged(nameof(ConfirmNewPassword));
            }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged(nameof(IsSaving));
            }
        }

        // View подписывается, чтобы очистить PasswordBox-ы (не биндятся напрямую)
        public event Action? PasswordChangeSucceeded;

        // Список команд
        public RelayCommand SelectSectionCommand { get; }
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand OpenGuideCommand { get; }

        public SettingsViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            _auth = AuthService.Instance();
            _auth.AuthChanged += OnAuthChanged;

            CanEdit = _auth.IsAdmin;

            SelectSectionCommand = new RelayCommand(param =>
            {
                if (param is string section)
                {
                    // "Password" и "Log" — только для админа. Проверяем и тут,
                    // а не только скрытием кнопки в XAML — на случай прямого вызова.
                    if ((section == "Password" || section == "Log") && !CanEdit)
                        return Task.CompletedTask;

                    SelectedSection = section;
                }

                return Task.CompletedTask;
            });

            BackToMenuCommand = new RelayCommand(_ =>
            {
                SelectedSection = "Menu";
                return Task.CompletedTask;
            });

            ChangePasswordCommand = new RelayCommand(async _ => await ChangePasswordAsync());

            OpenGuideCommand = new RelayCommand(_ =>
            {
                _hub.ShowGuide();
                return Task.CompletedTask;
            });

        }

        private void OnAuthChanged()
        {
            CanEdit = _auth.IsAdmin;

            // если права пропали (например, вышли из админки), а пользователь
            // был в закрытом разделе — возвращаем в меню
            if (!CanEdit && (SelectedSection == "Password" || SelectedSection == "Log"))
                SelectedSection = "Menu";
        }

        private async Task ChangePasswordAsync()
        {
            if (IsSaving)
                return;

            if (string.IsNullOrWhiteSpace(OldPassword))
            {
                InfoService.Show("Введите текущий пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                InfoService.Show("Введите новый пароль");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                InfoService.Show("Новый пароль и подтверждение не совпадают");
                return;
            }

            try
            {
                IsSaving = true;

                var (success, result) = await _apiService.PutAsyncSafe<ChangePasswordResponse>(
                    "MuseumInfo/password",
                    new { oldPassword = OldPassword, newPassword = NewPassword });

                if (success)
                {
                    InfoService.Show("Пароль успешно изменён");

                    OldPassword = "";
                    NewPassword = "";
                    ConfirmNewPassword = "";

                    PasswordChangeSucceeded?.Invoke();

                    SelectedSection = "Menu";
                }
                else
                {
                    var message = result?.Message switch
                    {
                        "OLD_PASSWORD_INVALID" => "Текущий пароль указан неверно",
                        "NEW_PASSWORD_EMPTY" => "Новый пароль не может быть пустым",
                        "NEW_PASSWORD_TOO_SHORT" => "Новый пароль должен быть не короче 6 символов",
                        _ => "Не удалось изменить пароль"
                    };

                    InfoService.Show(message);
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка смены пароля:\n{ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}