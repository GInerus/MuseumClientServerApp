using System.Windows;
using MeseumClient.Services;
using MeseumClient.ViewModels; // для LoginViewModel
using MeseumClient.Views;      // для LoginView

namespace MeseumClient.Views
{
    public partial class MainWindow : Window
    {
        private readonly SessionService _sessionService;

        public MainWindow()
        {
            InitializeComponent();

            // Создаём SessionService (он сам берёт IP/порт из appsettings.json)
            _sessionService = new SessionService();

            // Создаём LoginView
            var loginView = new LoginView(_sessionService);

            // Подписываемся на событие LoginSucceeded из ViewModel LoginView
            if (loginView.DataContext is LoginViewModel vm)
            {
                vm.LoginSucceeded += ShowMainView;
            }

            // Вставляем LoginView в MainGrid
            MainGrid.Children.Add(loginView);
        }

        public void ShowMainView(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            MainGrid.Children.Clear();

            // Передаём сервис, а не токен
            var mainView = new MainView(_sessionService);
            MainGrid.Children.Add(mainView);
        }
    }
}