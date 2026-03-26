using MeseumClient.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MeseumClient.ViewModels
{
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

        private object _currentView = null!;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowExhibitsCommand { get; }

        public MainViewModel(string userType)
        {
            UserRole = userType; // автоматически переводится

            // команды
            ShowExhibitsCommand = new RelayCommand(() =>
            {
                CurrentView = new ExhibitsViewModel();
            });

            // стартовый экран
            CurrentView = new ExhibitsViewModel();
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
    }
}