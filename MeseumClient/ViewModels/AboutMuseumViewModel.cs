using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace MeseumClient.ViewModels
{
    public class AboutMuseumViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _articlesCount;
        private int _departmentsCount;
        private int _exhibitsCount;
        private int _mediaCount;

        private string _token = "";

        public int ArticlesCount
        {
            get => _articlesCount;
            set { _articlesCount = value; OnPropertyChanged(); }
        }

        public int DepartmentsCount
        {
            get => _departmentsCount;
            set { _departmentsCount = value; OnPropertyChanged(); }
        }

        public int ExhibitsCount
        {
            get => _exhibitsCount;
            set { _exhibitsCount = value; OnPropertyChanged(); }
        }

        public int MediaCount
        {
            get => _mediaCount;
            set { _mediaCount = value; OnPropertyChanged(); }
        }

        public AboutMuseumViewModel(string token = "")
        {
            _token = token;

            if (!string.IsNullOrEmpty(_token))
            {
                _ = LoadDataAsync();
            }
        }

        // Устанавливаем токен позднее
        public void SetTokenAndLoad(string token)
        {
            _token = token;

            MessageBox.Show($"[SetTokenAndLoad] Токен установлен: {_token}");

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("[LoadDataAsync] Токен пустой, загрузка не выполняется.");
                return;
            }

            MessageBox.Show("[LoadDataAsync] Начинаем загрузку данных...");

            try
            {
                using var client = new HttpClient();

                // 🔹 Исправлено: токен в заголовке 'token'
                client.DefaultRequestHeaders.Remove("token");
                client.DefaultRequestHeaders.Add("token", _token);

                ArticlesCount = await GetCountAsync(client, "Document");
                DepartmentsCount = await GetCountAsync(client, "Departments");
                ExhibitsCount = await GetCountAsync(client, "Exhibit");
                MediaCount = await GetCountAsync(client, "MediaFiles");

                MessageBox.Show($"[LoadDataAsync] Данные загружены:\n" +
                                $"Articles: {ArticlesCount}\n" +
                                $"Departments: {DepartmentsCount}\n" +
                                $"Exhibits: {ExhibitsCount}\n" +
                                $"Media: {MediaCount}");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"[LoadDataAsync] Ошибка HTTP: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"[LoadDataAsync] Общая ошибка: {ex.Message}");
            }
        }

        private async Task<int> GetCountAsync(HttpClient client, string endpoint)
        {
            try
            {
                var response = await client.GetFromJsonAsync<CountResponse>(
                    $"https://localhost:7093/api/{endpoint}/count",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response != null)
                {
                    MessageBox.Show($"[GetCountAsync] {endpoint}: status={response.status}, count={response.count}");
                    if (response.status == "ok") return response.count;
                }
                else
                {
                    MessageBox.Show($"[GetCountAsync] {endpoint}: response пустой");
                }

                return 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"[GetCountAsync] {endpoint} ошибка: {ex.Message}");
                return 0;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private class CountResponse
        {
            public string status { get; set; } = "";
            public int count { get; set; }
        }
    }
}