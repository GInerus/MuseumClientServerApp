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
        private string _departmentCount = "Загрузка...";
        private string _documentCount = "Загрузка...";
        private string _exhibitCount = "Загрузка...";
        private string _mediaFileCount = "Загрузка...";

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
            get => _departmentCount;
            set
            {
                _departmentCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DepartmentCount)));
            }
        }

        public string DocumentCount
        {
            get => _documentCount;
            set
            {
                _documentCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DocumentCount)));
            }
        }

        public string ExhibitCount
        {
            get => _exhibitCount;
            set
            {
                _exhibitCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExhibitCount)));
            }
        }

        public string MediaFileCount
        {
            get => _mediaFileCount;
            set
            {
                _mediaFileCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MediaFileCount)));
            }
        }

        public async Task LoadDepartmentCountAsync()
        {
            try
            {
                var departmentTask = _apiService.GetAsync<CountResponse>("Department/count");
                var documentTask = _apiService.GetAsync<CountResponse>("Document/count");
                var exhibitTask = _apiService.GetAsync<CountResponse>("Exhibit/count");
                var mediaTask = _apiService.GetAsync<CountResponse>("MediaFile/count");

                await Task.WhenAll(departmentTask, documentTask, exhibitTask, mediaTask);

                DepartmentCount = (await departmentTask).Count.ToString();
                DocumentCount = (await documentTask).Count.ToString();
                ExhibitCount = (await exhibitTask).Count.ToString();
                MediaFileCount = (await mediaTask).Count.ToString();
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