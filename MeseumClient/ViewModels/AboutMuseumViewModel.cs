using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MeseumClient.ViewModels
{
    public class AboutMuseumViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _articlesCount;
        public int ArticlesCount
        {
            get => _articlesCount;
            set { _articlesCount = value; OnPropertyChanged(); }
        }

        private int _departmentsCount;
        public int DepartmentsCount
        {
            get => _departmentsCount;
            set { _departmentsCount = value; OnPropertyChanged(); }
        }

        private int _exhibitsCount;
        public int ExhibitsCount
        {
            get => _exhibitsCount;
            set { _exhibitsCount = value; OnPropertyChanged(); }
        }

        private int _mediaCount;
        public int MediaCount
        {
            get => _mediaCount;
            set { _mediaCount = value; OnPropertyChanged(); }
        }

        private readonly string _token;

        public AboutMuseumViewModel(string token)
        {
            _token = token;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token);

                ArticlesCount = await GetCountAsync(client, "Articles");
                DepartmentsCount = await GetCountAsync(client, "Departments");
                ExhibitsCount = await GetCountAsync(client, "Exhibits");
                MediaCount = await GetCountAsync(client, "Media");
            }
            catch
            {
                // Заглушка на случай ошибки
                ArticlesCount = DepartmentsCount = ExhibitsCount = MediaCount = 0;
            }
        }

        private async Task<int> GetCountAsync(HttpClient client, string endpoint)
        {
            try
            {
                var response = await client.GetFromJsonAsync<CountResponse>(
                    $"https://localhost:7093/api/{endpoint}/count");

                if (response != null && response.status == "ok")
                    return response.count;

                return 0;
            }
            catch
            {
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