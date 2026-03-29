using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels
{
    public class AboutMuseumViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public AboutMuseumViewModel()
        {
            // Получаем настройки сервера из ConfigService
            var config = new ConfigService().Server;

            // Создаём ApiService с токеном из AuthService
            _apiService = new ApiService(config, AuthService.Instance());
        }

        private string _text = "Загрузка...";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public async Task LoadDepartmentCountAsync()
        {
            try
            {
                // Вызываем GET /api/Department/count
                var countResponse = await _apiService.GetAsync<CountResponse>("Department/count");

                // Сохраняем число в свойство Text
                Text = $"Количество отделов: {countResponse.Count}";
            }
            catch (Exception ex)
            {
                Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}