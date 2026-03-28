using System.ComponentModel;
using MuseumClient.ViewModels;

namespace MuseumClient.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
            }
        }

        public LoginViewModel LoginVM { get; }
        public TokenViewModel TokenVM { get; }

        public MainViewModel()
        {
            LoginVM = new LoginViewModel(this); // передаем MainViewModel для навигации
            TokenVM = new TokenViewModel();

            CurrentView = LoginVM; // по умолчанию открываем экран логина
        }

        public void ShowTokenView()
        {
            CurrentView = TokenVM;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}