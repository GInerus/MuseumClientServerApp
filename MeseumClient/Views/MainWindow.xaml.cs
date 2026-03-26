using System.Windows;
using MeseumClient.Services;

namespace MeseumClient.Views
{
    public partial class MainWindow : Window
    {
        private readonly SessionService _sessionService;

        public MainWindow()
        {
            InitializeComponent();

            // Создаём сервисы
            var discovery = new NetworkDiscoveryService();
            var tcp = new TcpClientService();
            _sessionService = new SessionService(tcp);
            // Вставляем LoginView в MainGrid
            var loginView = new LoginView(_sessionService);
            MainGrid.Children.Add(loginView);
        }

        public void ShowMainView(string userType)
        {
            MainGrid.Children.Clear(); // убираем LoginView

            // Создаём новый UserControl для “главной панели”
            var mainView = new MainView(userType); // передаем admin/guest
            MainGrid.Children.Add(mainView);
        }
    }
}