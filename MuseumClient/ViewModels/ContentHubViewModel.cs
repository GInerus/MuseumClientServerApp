using System.ComponentModel;
using MuseumClient.Services;

namespace MuseumClient.ViewModels
{
    public class ContentHubViewModel : INotifyPropertyChanged
    {
        // Получаем токен напрямую из Singleton AuthService
        public string Text => AuthService.Instance().CurrentToken;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}