using System.Net.Http;
using System.Net.Http.Json;

namespace MuseumClient.Services
{
    public class ApiService
    {
        private readonly AuthService _authService;
        private readonly HttpClient _client;

        public ApiService(ServerConfig config, AuthService authService)
        {
            _authService = authService;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler);
        }

        private void ApplyHeaders()
        {
            _client.DefaultRequestHeaders.Clear();

            var token = _authService.CurrentToken;

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add("token", token);
            }
        }

        private string BuildUrl(string endpoint)
        {
            return $"{_authService.BaseUrl}/api/{endpoint}";
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            ApplyHeaders();

            var response = await _client.GetAsync(BuildUrl(endpoint));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<byte[]> GetBytesAsync(string endpoint)
        {
            ApplyHeaders();

            var response = await _client.GetAsync(BuildUrl(endpoint));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<T> PostAsync<T>(string endpoint, object payload)
        {
            ApplyHeaders();

            var response = await _client.PostAsJsonAsync(BuildUrl(endpoint), payload);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutAsync<T>(string endpoint, object payload)
        {
            ApplyHeaders();

            var response = await _client.PutAsJsonAsync(BuildUrl(endpoint), payload);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            ApplyHeaders();

            var response = await _client.DeleteAsync(BuildUrl(endpoint));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }


        public async Task<bool> DeleteAsync(string endpoint)
        {
            ApplyHeaders();

            var response = await _client.DeleteAsync(BuildUrl(endpoint));

            return response.IsSuccessStatusCode;
        }

        public async Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            ApplyHeaders();

            var response = await _client.PostAsync(
                BuildUrl(endpoint),
                content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            ApplyHeaders();
            var response = await _client.PutAsync(BuildUrl(endpoint), content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<(bool Success, T? Data)> PutAsyncSafe<T>(string endpoint, object payload)
        {
            ApplyHeaders();
            var response = await _client.PutAsJsonAsync(BuildUrl(endpoint), payload);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return (response.IsSuccessStatusCode, data);
        }

        public async Task<(bool Success, T? Data)> PostAsyncSafe<T>(string endpoint, object payload)
        {
            ApplyHeaders();
            var response = await _client.PostAsJsonAsync(BuildUrl(endpoint), payload);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return (response.IsSuccessStatusCode, data);
        }

        public async Task<byte[]> PostBytesAsync(string endpoint, object payload)
        {
            ApplyHeaders();
            var response = await _client.PostAsJsonAsync(BuildUrl(endpoint), payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}