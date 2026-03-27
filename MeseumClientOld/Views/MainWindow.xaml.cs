using System.Net.Http;
using System.Windows;
using MeseumClient.Config;
using MeseumClient.Services;
using MeseumClient.ViewModels;
using MeseumClient.Views;

namespace MeseumClient.Views
{
    public partial class MainWindow : Window
    {
        private readonly SessionService _sessionService;

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем конфиг через AppSettings
            var appSettings = AppSettings.Load();
            var serverConfig = appSettings.Server;

            // Создаём HttpClient
            var httpClient = new HttpClient();

            // Создаём сервис с готовым конфигом
            _sessionService = new SessionService(httpClient, serverConfig);

            // Создаём LoginView
            var loginView = new LoginView(_sessionService);

            // Подписываемся на событие LoginSucceeded
            if (loginView.DataContext is LoginViewModel vm)
                vm.LoginSucceeded += ShowMainView;

            MainGrid.Children.Add(loginView);
        }

        public void ShowMainView(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            MainGrid.Children.Clear();
            MainGrid.Children.Add(new MainView(_sessionService));
        }
    }
}