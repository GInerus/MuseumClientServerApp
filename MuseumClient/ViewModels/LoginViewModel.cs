using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using MuseumClient.Commands;
using MuseumClient.Services;
using MuseumClient.Helpers;

namespace MuseumClient.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainVM;
        private readonly AuthService _authService;

        public string UserType { get; set; }

        public RelayCommand RegisterCommand { get; }

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

        public LoginViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;
            _authService = new AuthService(new Services.ConfigService().Server);

            RegisterCommand = new RelayCommand(async _ =>
            {
                bool success = await _authService.RegisterAsync(UserType, UserPassword);
                if (success)
                {
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