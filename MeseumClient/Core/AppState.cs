using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeseumClient.Core
{
    public class AppState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _userType = "guest";
        public string UserType
        {
            get => _userType;
            set { _userType = value; OnPropertyChanged(); }
        }

        private string? _token;
        public string? Token
        {
            get => _token;
            set { _token = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}