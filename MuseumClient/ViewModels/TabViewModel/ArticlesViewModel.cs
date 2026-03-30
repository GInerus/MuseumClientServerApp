using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MuseumClient.Commands;
using System.Windows;
using MuseumClient.Views;


namespace MuseumClient.ViewModels
{
    public class ArticlesViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public RelayCommand LoadCommand { get; }

        public ArticlesViewModel() 
        {
            // Получаем настройки сервера из ConfigService
            var config = new ConfigService().Server;

            // Создаём ApiService с токеном из AuthService
            _apiService = new ApiService(config, AuthService.Instance());
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
