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

        public void Save()
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
                "AppSettings.json",
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