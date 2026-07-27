using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

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

        // ===== Журнал действий =====

        public ObservableCollection<LogEntryDto> Logs { get; } = new();

        private bool _isLoadingLogs;
        public bool IsLoadingLogs
        {
            get => _isLoadingLogs;
            set
            {
                _isLoadingLogs = value;
                OnPropertyChanged(nameof(IsLoadingLogs));
            }
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                IsLoadingLogs = true;

                var response = await _apiService.GetAsync<LogsResponse>("Log");

                Logs.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        Logs.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки журнала:\n{ex.Message}");
            }
            finally
            {
                IsLoadingLogs = false;
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

            SelectSectionCommand = new RelayCommand(async param =>
            {
                if (param is string section)
                {
                    // "Password" и "Log" — только для админа. Проверяем и тут,
                    // а не только скрытием кнопки в XAML — на случай прямого вызова.
                    if ((section == "Password" || section == "Log") && !CanEdit)
                        return;

                    SelectedSection = section;

                    if (section == "Log")
                    {
                        await LoadLogsAsync();
                    }

                    if (section == "Backup")
                    {
                        await LoadBackupsAsync();
                        await LoadBackupSettingsAsync();
                    }
                }
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

            CreateFullBackupCommand =
                new RelayCommand(async _ => await CreateFullBackupAsync());

            CreateDifferentialBackupCommand =
                new RelayCommand(async _ => await CreateDifferentialBackupAsync());

            RefreshBackupsCommand =
                new RelayCommand(async _ => await LoadBackupsAsync());

            RestoreBackupCommand =
                new RelayCommand(async _ => await RestoreBackupAsync());

            DeleteBackupCommand =
                new RelayCommand(async _ => await DeleteBackupAsync());

            SaveBackupSettingsCommand =
                new RelayCommand(async _ => await SaveBackupSettingsAsync());

        }

        private void OnAuthChanged()
        {
            CanEdit = _auth.IsAdmin;

            // если права пропали (например, вышли из админки), а пользователь
            // был в закрытом разделе — возвращаем в меню
            if (!CanEdit && (SelectedSection == "Password" || SelectedSection == "Log" || SelectedSection == "Backup"))
            {
                SelectedSection = "Menu";
            }
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

        // ===== Резервное копирование =====

        public ObservableCollection<BackupPointDto> Backups { get; } = new();

        private BackupPointDto? _selectedBackup;
        public BackupPointDto? SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                _selectedBackup = value;
                OnPropertyChanged(nameof(SelectedBackup));
            }
        }

        private BackupSettingsDto _backupSettings = new();
        public BackupSettingsDto BackupSettings
        {
            get => _backupSettings;
            set
            {
                _backupSettings = value;
                OnPropertyChanged(nameof(BackupSettings));
            }
        }

        private bool _restoreMedia = true;
        public bool RestoreMedia
        {
            get => _restoreMedia;
            set
            {
                _restoreMedia = value;
                OnPropertyChanged(nameof(RestoreMedia));
            }
        }

        private bool _isBackupBusy;
        public bool IsBackupBusy
        {
            get => _isBackupBusy;
            set
            {
                _isBackupBusy = value;
                OnPropertyChanged(nameof(IsBackupBusy));
            }
        }

        // Список команд

        public RelayCommand CreateFullBackupCommand { get; }
        public RelayCommand CreateDifferentialBackupCommand { get; }
        public RelayCommand RefreshBackupsCommand { get; }
        public RelayCommand RestoreBackupCommand { get; }
        public RelayCommand DeleteBackupCommand { get; }
        public RelayCommand SaveBackupSettingsCommand { get; }

        private async Task LoadBackupsAsync()
        {
            try
            {
                IsBackupBusy = true;

                var response = await _apiService.GetAsync<BackupListResponse>("Backup");

                Backups.Clear();

                if (response?.Data != null)
                {
                    foreach (var item in response.Data.OrderByDescending(x => x.CreatedAt))
                    {
                        Backups.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки резервных копий:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }

        private async Task LoadBackupSettingsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<BackupSettingsResponse>("Backup/settings");

                if (response?.Data != null)
                    BackupSettings = response.Data;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка загрузки настроек:\n{ex.Message}");
            }
        }

        private async Task CreateFullBackupAsync()
        {
            if (!ConfirmService.Show("Создать полный резервный бэкап?\n\nБудет создана резервная копия базы данных и медиафайлов."))
            {
                return;
            }

            try
            {
                IsBackupBusy = true;

                await _apiService.PostAsync<object>(
                    "Backup/full",
                    new { });

                await LoadBackupsAsync();

                InfoService.Show("Полный резервный бэкап успешно создан.");
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка создания полного бэкапа:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }
        private async Task CreateDifferentialBackupAsync()
        {
            if (!ConfirmService.Show("Создать разностный резервный бэкап?\n\nБудут сохранены изменения базы данных с момента последнего полного бэкапа."))
            {
                return;
            }

            try
            {
                IsBackupBusy = true;

                await _apiService.PostAsync<object>(
                    "Backup/differential",
                    new { });

                await LoadBackupsAsync();

                InfoService.Show("Разностный резервный бэкап успешно создан.");
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка создания разностного бэкапа:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }

        private async Task RestoreBackupAsync()
        {
            if (SelectedBackup == null)
            {
                InfoService.Show("Выберите резервную копию.");
                return;
            }

            if (!ConfirmService.Show($"Восстановить резервную копию?\n\n{SelectedBackup.BaseName}\n\nТекущая база данных будет заменена."))
            {
                return;
            }

            try
            {
                IsBackupBusy = true;

                await _apiService.PostAsync<object>(
                    "Backup/restore",
                    new RestoreBackupRequest
                    {
                        BaseName = SelectedBackup.BaseName,
                        RestoreMedia = RestoreMedia
                    });

                InfoService.Show("Восстановление завершено.");

                await LoadBackupsAsync();
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка восстановления:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }

        private async Task DeleteBackupAsync()
        {
            if (SelectedBackup == null)
            {
                InfoService.Show("Выберите резервную копию.");
                return;
            }

            if (!ConfirmService.ConfirmDelete($"резервную копию \"{SelectedBackup.BaseName}\""))
            {
                return;
            }

            try
            {
                IsBackupBusy = true;

                await _apiService.DeleteAsync(
                    $"Backup/{SelectedBackup.BaseName}");

                Backups.Remove(SelectedBackup);

                SelectedBackup = null;

                InfoService.Show("Резервная копия удалена.");
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка удаления:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }

        private async Task SaveBackupSettingsAsync()
        {
            try
            {
                IsBackupBusy = true;

                await _apiService.PutAsync<object>(
                    "Backup/settings",
                    new UpdateBackupSettingsRequest
                    {
                        BackupRetentionDays = BackupSettings.BackupRetentionDays,
                        BackupFullDayOfWeek = BackupSettings.BackupFullDayOfWeek,
                        BackupFullTime = BackupSettings.BackupFullTime,
                        BackupDifferentialTime = BackupSettings.BackupDifferentialTime
                    });

                InfoService.Show("Настройки резервного копирования сохранены.");
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка сохранения:\n{ex.Message}");
            }
            finally
            {
                IsBackupBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}