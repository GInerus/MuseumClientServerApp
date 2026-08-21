using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MuseumClient.Services
{
    public class ConfigService
    {
        public ServerConfig Server { get; private set; }

        public ConfigService()
        {
            var json = File.ReadAllText("AppSettings.json");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Server = new ServerConfig
            {
                LocalUrl = root.GetProperty("Server").GetProperty("LocalUrl").GetString(),
                RemoteUrl = root.GetProperty("Server").GetProperty("RemoteUrl").GetString()
            };
        }

        public bool Save()
        {
            try
            {
                SaveToFile();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public void SaveWithAdministrator()
        {
            var exePath = Environment.ProcessPath;

            if (string.IsNullOrWhiteSpace(exePath))
                throw new InvalidOperationException("Не удалось определить путь к программе.");

            string localUrl = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(Server.LocalUrl));

            string remoteUrl = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(Server.RemoteUrl));

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"--save-settings \"{localUrl}\" \"{remoteUrl}\"",
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        public void SaveToFile()
        {
            var root = new JsonObject
            {
                ["Server"] = new JsonObject
                {
                    ["LocalUrl"] = Server.LocalUrl,
                    ["RemoteUrl"] = Server.RemoteUrl
                }
            };

            File.WriteAllText(
                Path.Combine(AppContext.BaseDirectory, "AppSettings.json"),
                root.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }
    }

    public class ServerConfig
    {
        public string LocalUrl { get; set; }
        public string RemoteUrl { get; set; }
    }
}