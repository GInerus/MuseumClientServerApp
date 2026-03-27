using MeseumClient.Commands;
using MeseumClient.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MeseumClient.ViewModels
{
    public class TabItemViewModel
    {
        public string Header { get; set; } = "";
        public object Content { get; set; } = null!;
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _welcomeMessage = "";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set { _welcomeMessage = value; OnPropertyChanged(); }
        }

        private string _userRole = "";
        public string UserRole
        {
            get => _userRole;
            set { _userRole = TranslateRole(value); OnPropertyChanged(); }
        }

        public ObservableCollection<TabItemViewModel> Tabs { get; set; } = new();

        private TabItemViewModel _currentTab = null!;
        public TabItemViewModel CurrentTab
        {
            get => _currentTab;
            set { _currentTab = value; OnPropertyChanged(); }
        }

        // Команды для сайдбара
        public ICommand ShowAboutTabCommand { get; }
        public ICommand ShowExhibitsTabCommand { get; }

        // Команда для панели вкладок
        public ICommand SelectTabCommand { get; }

        public MainViewModel(string token)
        {
            // Сразу создаём вкладки
            var aboutTab = new TabItemViewModel
            {
                Header = "О музее",
                Content = new AboutMuseumView()
            };
            Tabs.Add(aboutTab);

            var exhibitsTab = new TabItemViewModel
            {
                Header = "Экспонаты",
                Content = new ExhibitsView()
            };
            Tabs.Add(exhibitsTab);

            // По умолчанию открыта вкладка "О музее"
            CurrentTab = aboutTab;

            // Команды для сайдбара
            ShowAboutTabCommand = new RelayCommand(() => CurrentTab = aboutTab);
            ShowExhibitsTabCommand = new RelayCommand(() => CurrentTab = exhibitsTab);

            // Команда для панели вкладок
            SelectTabCommand = new RelayCommand(() => { /* через CommandParameter в XAML */ });

            // Асинхронно валидируем токен и получаем роль пользователя
            _ = InitializeUserAsync(token);
        }

        private async Task InitializeUserAsync(string token)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<SessionValidateResponse>(
                    $"https://localhost:7093/api/Session/validate/{token}");

                if (response?.status == "ok" && !string.IsNullOrEmpty(response.userType))
                {
                    UserRole = TranslateRole(response.userType);
                }
                else
                {
                    UserRole = "Гость";
                }
            }
            catch
            {
                UserRole = "Гость";
            }
        }

        private string TranslateRole(string role)
        {
            return role.ToLower() switch
            {
                "guest" => "Гость",
                "admin" => "Администратор",
                _ => role
            };
        }

        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private class SessionValidateResponse
        {
            public string status { get; set; } = "";
            public string userType { get; set; } = "";
        }
    }
}