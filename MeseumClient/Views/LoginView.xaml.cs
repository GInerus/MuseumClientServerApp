using MeseumClient.Services;
using MeseumClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MeseumClient.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView(SessionService sessionService)
        {
            InitializeComponent();
            var vm = new LoginViewModel(sessionService);
            this.DataContext = vm;

            // Подписка на успешный вход
            vm.LoginSucceeded += UserLoggedIn;
        }

        // Метод вызывается при успешном логине
        private void UserLoggedIn(string userType)
        {
            // Сообщаем MainWindow
            (Window.GetWindow(this) as MainWindow)?.ShowMainView(userType);
        }
    }
}