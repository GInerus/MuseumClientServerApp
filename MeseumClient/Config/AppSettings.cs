using System.IO;
using System.Text.Json;

namespace MeseumClient.Config
{
    public class AppSettings
    {
        public ServerConfig Server { get; set; } = null!;

        public static AppSettings Load(string path = "appsettings.json")
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл конфигурации {path} не найден");

            var jsonText = File.ReadAllText(path);
            try
            {
                return JsonSerializer.Deserialize<AppSettings>(jsonText)
                       ?? throw new Exception("Не удалось десериализовать конфигурацию");
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при чтении конфигурации", ex);
            }
        }
    }
}