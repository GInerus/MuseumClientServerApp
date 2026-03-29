using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuseumClient.Commands;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class AboutMuseumViewModel : INotifyPropertyChanged
    {
        private string _text = "Загрузка...";

        private readonly ApiService _apiService;

        public RelayCommand LoadCommand { get; }

        public AboutMuseumViewModel()
        {
            // Получаем настройки сервера из ConfigService
            var config = new ConfigService().Server;

            // Создаём ApiService с токеном из AuthService
            _apiService = new ApiService(config, AuthService.Instance());

            LoadCommand = new RelayCommand(async _ => await LoadDepartmentCountAsync());
        }

        public string DepartmentCount
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DepartmentCount)));
            }
        }

        public string DocumentCount
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentCount)));
            }
        }

        public string ExhibitCount
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExhibitCount)));
            }
        }

        public string MediaFileCount
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaFileCount)));
            }
        }

        public async Task LoadDepartmentCountAsync()
        {
            try
            {
                // Вызываем GET /api/Department/count
                var departmentCountResponse = await _apiService.GetAsync<CountResponse>("Department/count");
                // Вызываеем GET /api/Document/count
                var documentCountResponse = await _apiService.GetAsync<CountResponse>("Document/count");
                // Вызываем GET /api/Exhibit/count
                var exhibitCountResponse = await _apiService.GetAsync<CountResponse>("Exhibit/count");
                // Вызываеем GET /api/MediaFile/count
                var mediaFileCountResponse = await _apiService.GetAsync<CountResponse>("MediaFile/count");

                // Сохраняем числа в свойство Text
                DepartmentCount = (departmentCountResponse.Count).ToString();
                DocumentCount = (documentCountResponse.Count).ToString();
                ExhibitCount = (exhibitCountResponse.Count).ToString();
                MediaFileCount = (mediaFileCountResponse.Count).ToString();
            }
            catch (Exception ex)
            {
                DepartmentCount = $"Ошибка загрузки: {ex.Message}";
                DocumentCount = $"Ошибка загрузки: {ex.Message}";
                ExhibitCount = $"Ошибка загрузки: {ex.Message}";
                MediaFileCount = $"Ошибка загрузки: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}