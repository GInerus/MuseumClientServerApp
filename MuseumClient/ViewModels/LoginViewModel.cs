using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using MuseumClient.Commands;
using MuseumClient.Services;

namespace MuseumClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainVM;
        private readonly AuthService _authService;

        public string UserType { get; set; }

        public RelayCommand RegisterCommand { get; }

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _authService = new AuthService(new Services.ConfigService().Server);

            RegisterCommand = new RelayCommand(async param =>
            {
                string password = param as string;
                bool success = await _authService.RegisterAsync(UserType, password);
                if (success)
                {
                    // после успешной регистрации переключаем экран
                    _mainVM.TokenVM.SetToken(_authService.CurrentToken);
                    _mainVM.ShowTokenView();
                }
                else
                {
                    MessageBox.Show("Registration failed");
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}