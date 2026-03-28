using System.ComponentModel;

namespace MuseumClient.ViewModels
{
    public class ContentHubViewModel : INotifyPropertyChanged
    {
        private string _token;
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Token)));
            }
        }

        public void SetToken(string token)
        {
            Token = token;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}